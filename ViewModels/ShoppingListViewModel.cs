using System.Collections.ObjectModel;
using System.Windows.Input;
using ssk.Models;
using ssk.Services;
using ssk.Views;

namespace ssk.ViewModels;

public class ShoppingListViewModel : BaseViewModel
{
    private readonly ShoppingItemRepository _shoppingItemRepository;
    private readonly AuthService _authService;

    private string _newItemName = string.Empty;
    public string NewItemName
    {
        get => _newItemName;
        set => SetProperty(ref _newItemName, value);
    }

    private double _newItemQuantity;
    public double NewItemQuantity
    {
        get => _newItemQuantity;
        set => SetProperty(ref _newItemQuantity, value);
    }

    private string _newItemUnit = "克";
    public string NewItemUnit
    {
        get => _newItemUnit;
        set => SetProperty(ref _newItemUnit, value);
    }

    public ObservableCollection<ShoppingGroup> GroupedItems { get; } = new();

    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        set => SetProperty(ref _totalCount, value);
    }

    private int _purchasedCount;
    public int PurchasedCount
    {
        get => _purchasedCount;
        set => SetProperty(ref _purchasedCount, value);
    }

    private int _unpurchasedCount;
    public int UnpurchasedCount
    {
        get => _unpurchasedCount;
        set => SetProperty(ref _unpurchasedCount, value);
    }

    public ICommand AddItemCommand { get; }
    public ICommand TogglePurchasedCommand { get; }
    public ICommand DeleteItemCommand { get; }
    public ICommand ClearPurchasedCommand { get; }
    public ICommand LoginCommand { get; }

    public bool IsLoggedIn => _authService.IsLoggedIn;

    public ShoppingListViewModel(ShoppingItemRepository shoppingItemRepository, AuthService authService)
    {
        _shoppingItemRepository = shoppingItemRepository;
        _authService = authService;
        Title = "购物清单";

        _authService.LoginStateChanged += OnLoginStateChanged;

        AddItemCommand = CreateAsyncCommand(ExecuteAddItem);
        TogglePurchasedCommand = CreateAsyncCommand<ShoppingItem>(ExecuteTogglePurchased);
        DeleteItemCommand = CreateAsyncCommand<ShoppingItem>(ExecuteDeleteItem);
        ClearPurchasedCommand = CreateAsyncCommand(ExecuteClearPurchased);
        LoginCommand = CreateCommand(() => Shell.Current.GoToAsync(nameof(LoginPage)));
    }

    public async Task LoadItemsAsync()
    {
        try
        {
            var items = await _shoppingItemRepository.GetAllAsync();

            // 按分类分组
            var groups = items
                .GroupBy(i => i.Category)
                .Select(g => new ShoppingGroup(g.Key, g.ToList()))
                .OrderBy(g => g.Category)
                .ToList();

            GroupedItems.Clear();
            foreach (var group in groups)
                GroupedItems.Add(group);

            // 更新计数
            TotalCount = items.Count;
            PurchasedCount = items.Count(i => i.IsPurchased);
            UnpurchasedCount = items.Count(i => !i.IsPurchased);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShoppingListViewModel] LoadItemsAsync error: {ex.Message}");
        }
    }

    private async Task ExecuteAddItem()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewItemName))
            {
                await Shell.Current.DisplayAlert("提示", "请输入商品名称", "确定");
                return;
            }

            var item = new ShoppingItem
            {
                Name = NewItemName,
                Category = "蔬菜",
                Quantity = NewItemQuantity > 0 ? NewItemQuantity : 1,
                Unit = NewItemUnit,
                IsPurchased = false
            };

            await _shoppingItemRepository.SaveAsync(item);

            // 清空输入
            NewItemName = string.Empty;
            NewItemQuantity = 0;

            await LoadItemsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShoppingListViewModel] AddItem error: {ex.Message}");
        }
    }

    private async Task ExecuteTogglePurchased(ShoppingItem item)
    {
        try
        {
            await _shoppingItemRepository.MarkPurchasedAsync(item, !item.IsPurchased);
            await LoadItemsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShoppingListViewModel] TogglePurchased error: {ex.Message}");
        }
    }

    private async Task ExecuteDeleteItem(ShoppingItem item)
    {
        try
        {
            await _shoppingItemRepository.DeleteAsync(item);
            await LoadItemsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShoppingListViewModel] DeleteItem error: {ex.Message}");
        }
    }

    private async Task ExecuteClearPurchased()
    {
        try
        {
            if (PurchasedCount == 0) return;

            var confirm = await Shell.Current.DisplayAlert("确认", $"确定要清除{PurchasedCount}个已购商品吗？", "清除", "取消");
            if (!confirm) return;

            await _shoppingItemRepository.ClearPurchasedAsync();
            await LoadItemsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShoppingListViewModel] ClearPurchased error: {ex.Message}");
        }
    }

    private void OnLoginStateChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(IsLoggedIn));
            if (_authService.IsLoggedIn)
                _ = LoadItemsAsync();
        });
    }
}
