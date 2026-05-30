using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using ssk.Models;
using SkiaSharp;

namespace ssk.Services;

public class YoloInferenceService
{
    private bool _modelLoaded;
    private string? _modelError;
    private InferenceSession? _session;
    private int _inputWidth = 640;
    private int _inputHeight = 640;
    private bool _initializing;

    public bool IsModelLoaded => _modelLoaded;
    public string? ModelError => _modelError;

    private static readonly Dictionary<string, string> LabelMap = new()
    {
        ["banana"] = "香蕉", ["apple"] = "苹果", ["orange"] = "橙子",
        ["pizza"] = "披萨", ["cake"] = "蛋糕", ["hot dog"] = "热狗",
        ["sandwich"] = "三明治", ["broccoli"] = "西兰花", ["carrot"] = "胡萝卜",
        ["bowl"] = "碗装食物", ["dining table"] = "餐桌", ["refrigerator"] = "冰箱",
        ["cup"] = "杯子", ["bottle"] = "瓶子", ["wine glass"] = "酒杯",
        ["fork"] = "叉子", ["knife"] = "刀", ["spoon"] = "勺子",
        ["microwave"] = "微波炉", ["oven"] = "烤箱", ["toaster"] = "烤面包机",
        ["sink"] = "水槽"
    };

    public async Task InitializeAsync()
    {
        if (_initializing) return;
        _initializing = true;

        try
        {
            var modelPath = await ResolveModelPathAsync("yolov8n.onnx");
            if (modelPath == null)
            {
                _modelLoaded = false;
                _modelError = "YOLO模型文件未找到，请将 yolov8n.onnx 放入应用数据目录";
                return;
            }

            _session = new InferenceSession(modelPath);

            // 从模型元数据读取输入尺寸
            var inputMeta = _session.InputMetadata;
            var inputName = _session.InputNames.First();
            var shape = inputMeta[inputName].Dimensions;
            _inputHeight = shape[2] > 0 ? shape[2] : 640;
            _inputWidth = shape[3] > 0 ? shape[3] : 640;

            _modelLoaded = true;
            _modelError = null;
        }
        catch (Exception ex)
        {
            _modelError = $"模型加载失败: {ex.Message}";
            _modelLoaded = false;
        }
    }

    public async Task<List<YoloPrediction>> PredictAsync(string imagePath)
    {
        // 惰性初始化：首次调用时自动加载模型
        if (!_modelLoaded && _session == null)
            await InitializeAsync();

        if (!_modelLoaded || _session == null)
            throw new InvalidOperationException(_modelError ?? "模型未加载");

        return await Task.Run(() =>
        {
            // 1. 预处理图像
            var inputData = SkiaImagePreprocessor.PreprocessAsync(
                imagePath, _inputWidth).GetAwaiter().GetResult();

            // 2. 构造输入 tensor
            var inputTensor = new DenseTensor<float>(inputData, [1, 3, _inputHeight, _inputWidth]);
            var inputName = _session.InputNames.First();
            var inputNamedValue = NamedOnnxValue.CreateFromTensor(inputName, inputTensor);

            // 3. 执行推理
            using var results = _session.Run([inputNamedValue]);

            // 4. 提取输出张量
            var outputTensor = results.First().AsTensor<float>()?.ToArray() ?? [];

            // 5. 获取原图尺寸用于坐标还原
            int originalWidth, originalHeight;
            using (var bitmap = SKBitmap.Decode(imagePath))
            {
                originalWidth = bitmap?.Width ?? _inputWidth;
                originalHeight = bitmap?.Height ?? _inputHeight;
            }

            // 6. 后处理
            return YoloPostProcessor.ProcessOutput(
                outputTensor, originalWidth, originalHeight);
        });
    }

    /// <summary>
    /// 查找模型文件：优先从应用数据目录，回退到包内 Raw 资源并拷贝。
    /// </summary>
    private async Task<string?> ResolveModelPathAsync(string modelName)
    {
        // 优先：应用数据目录（侧载或之前拷贝的）
        var localPath = Path.Combine(FileSystem.AppDataDirectory, modelName);
        if (File.Exists(localPath))
            return localPath;

        // 回退：从包内 Raw 资源拷贝到应用数据目录
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(modelName);
            using var fileStream = File.OpenWrite(localPath);
            await stream.CopyToAsync(fileStream);
            return localPath;
        }
        catch
        {
            return null;
        }
    }

    public static string? MapLabelToChinese(string label)
    {
        return LabelMap.TryGetValue(label.ToLowerInvariant(), out var chinese) ? chinese : null;
    }
}
