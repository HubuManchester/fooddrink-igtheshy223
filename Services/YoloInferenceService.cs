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
        ["banana"] = "Banana", ["apple"] = "Apple", ["orange"] = "Orange",
        ["pizza"] = "Pizza", ["cake"] = "Cake", ["hot dog"] = "Hot Dog",
        ["sandwich"] = "Sandwich", ["broccoli"] = "Broccoli", ["carrot"] = "Carrot",
        ["bowl"] = "Bowl Food", ["dining table"] = "Dining Table", ["refrigerator"] = "Refrigerator",
        ["cup"] = "Cup", ["bottle"] = "Bottle", ["wine glass"] = "Wine Glass",
        ["fork"] = "Fork", ["knife"] = "Knife", ["spoon"] = "Spoon",
        ["microwave"] = "Microwave", ["oven"] = "Oven", ["toaster"] = "Toaster",
        ["sink"] = "Sink"
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
                _modelError = "YOLO model file not found, please put yolov8n.onnx in app data directory";
                return;
            }

            _session = new InferenceSession(modelPath);

            // Read input size from model metadata
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
            _modelError = $"Model load fail: {ex.Message}";
            _modelLoaded = false;
        }
    }

    public async Task<List<YoloPrediction>> PredictAsync(string imagePath)
    {
        // Lazy init: auto load model on first call
        if (!_modelLoaded && _session == null)
            await InitializeAsync();

        if (!_modelLoaded || _session == null)
            throw new InvalidOperationException(_modelError ?? "Model not loaded");

        return await Task.Run(() =>
        {
            // 1. Preprocess image
            var inputData = SkiaImagePreprocessor.PreprocessAsync(
                imagePath, _inputWidth).GetAwaiter().GetResult();

            // 2. Build input tensor
            var inputTensor = new DenseTensor<float>(inputData, [1, 3, _inputHeight, _inputWidth]);
            var inputName = _session.InputNames.First();
            var inputNamedValue = NamedOnnxValue.CreateFromTensor(inputName, inputTensor);

            // 3. Run inference
            using var results = _session.Run([inputNamedValue]);

            // 4. Extract output tensor
            var outputTensor = results.First().AsTensor<float>()?.ToArray() ?? [];

            // 5. Get original image size for coordinate restoration
            int originalWidth, originalHeight;
            using (var bitmap = SKBitmap.Decode(imagePath))
            {
                originalWidth = bitmap?.Width ?? _inputWidth;
                originalHeight = bitmap?.Height ?? _inputHeight;
            }

            // 6. Post process
            return YoloPostProcessor.ProcessOutput(
                outputTensor, originalWidth, originalHeight);
        });
    }

    /// <summary>
    /// Find model file: prefer app data directory, fallback to package Raw resource and copy.
    /// </summary>
    private async Task<string?> ResolveModelPathAsync(string modelName)
    {
        // Prefer: app data directory (sideloaded or previously copied)
        var localPath = Path.Combine(FileSystem.AppDataDirectory, modelName);
        if (File.Exists(localPath))
            return localPath;

        // Fallback: copy from package Raw resource to app data directory
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
        return LabelMap.TryGetValue(label.ToLowerInvariant(), out var english) ? english : null;
    }
}
