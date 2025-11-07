using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using InventorySystem.Data;
using InventorySystem;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem;

public partial class MainWindow : Window
{
    private readonly InventoryDbContext _db = new();   
    private readonly OrderBook _orderBook = new();
    private readonly ItemSorterRobot _robot = new();
    
    public ObservableCollection<Order> Queued    => _orderBook.Queued;
    public ObservableCollection<Order> Processed => _orderBook.Processed;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        _robot.Log = msg =>
            Dispatcher.UIThread.Post(() =>
                StatusMessages.Text += msg + Environment.NewLine);

        _robot.IpAddress = IpBox.Text ?? "127.0.0.1";
        _robot.DryRun    = UseDryRun.IsChecked ?? true;

        DbInitializer.Initialize(_db);

        LoadOrdersFromDatabase();

        UpdateTotals();
    }
    
    private void LoadOrdersFromDatabase()
    {
        var queuedOrders = _db.Orders
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Item)
            .Where(o => !o.IsProcessed)
            .OrderBy(o => o.Time)
            .ToList();

        var processedOrders = _db.Orders
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Item)
            .Where(o => o.IsProcessed)
            .OrderBy(o => o.Time)
            .ToList();

        _orderBook.Queued.Clear();
        _orderBook.Processed.Clear();

        foreach (var o in queuedOrders)
            _orderBook.Queued.Add(o);

        foreach (var o in processedOrders)
            _orderBook.Processed.Add(o);

    }

    private async void OnProcessClick(object? sender, RoutedEventArgs e)
    {
        StatusMessages.Text += "Processing order…" + Environment.NewLine;

        var lines = _orderBook.ProcessNextOrderAndReturnLines();

        if (lines == null || lines.Count == 0)
        {
            StatusMessages.Text += "No queued orders." + Environment.NewLine;
            return;
        }

        var processedOrder = _orderBook.LastProcessedOrder;   // forudsætter LastProcessedOrder i OrderBook
        if (processedOrder != null)
        {
            processedOrder.IsProcessed = true;
            _db.Orders.Update(processedOrder);
            _db.SaveChanges();
        }
        
        foreach (var line in lines)
        {
            for (int i = 0; i < line.Quantity; i++)
            {
                StatusMessages.Text +=
                    $"Picking up {line.Item.Name} (slot {line.Item.InventoryLocation})" +
                    Environment.NewLine;

                _robot.PickUp(line.Item.InventoryLocation);
                
                await Task.Delay(9500); 
            }
        }

        UpdateTotals();
    }

    private void UpdateTotals()
    {
        TotalText.Text = $"Total revenue: {_orderBook.TotalRevenue:0.00} kr.";
    }

    private void OnApplyRobotSettingsClick(object? sender, RoutedEventArgs e)
    {
        _robot.IpAddress = IpBox.Text ?? "127.0.0.1";
        _robot.DryRun    = UseDryRun.IsChecked ?? true;

        StatusMessages.Text +=
            $"Robot settings updated: IP={_robot.IpAddress}, DryRun={_robot.DryRun}{Environment.NewLine}";
    }
}

