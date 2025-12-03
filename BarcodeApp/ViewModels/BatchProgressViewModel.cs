using System;
using System.Threading;
using System.Windows.Input;
using BarcodeApp.Models;
using BarcodeApp.ViewModels;

namespace BarcodeApp.ViewModels;

public class BatchProgressViewModel : BaseViewModel
{
    private PrintProgress _progress = new();
    private bool _isPrinting;
    private CancellationTokenSource? _cancellationTokenSource;

    public BatchProgressViewModel()
    {
        CancelCommand = new RelayCommand(_ => Cancel(), _ => IsPrinting);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public PrintProgress Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    public bool IsPrinting
    {
        get => _isPrinting;
        set => SetProperty(ref _isPrinting, value);
    }

    public CancellationToken CancellationToken => _cancellationTokenSource?.Token ?? CancellationToken.None;

    public ICommand CancelCommand { get; }

    public event EventHandler? CancelRequested;

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateProgress(int current, int total, string currentItemData, string status)
    {
        Progress = new PrintProgress
        {
            CurrentItem = current,
            TotalItems = total,
            CurrentItemData = currentItemData,
            Status = status
        };
    }

    public void Reset()
    {
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        Progress = new PrintProgress();
        IsPrinting = false;
    }
}

