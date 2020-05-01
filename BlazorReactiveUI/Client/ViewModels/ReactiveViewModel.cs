using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using BlazorReactiveUI.Client.Reactive;

namespace BlazorReactiveUI.Client.ViewModels
{
    public class ReactiveViewModel : ReactiveObject, IDisposable
    {
        public bool Running { [ObservableAsProperty] get; }
        [Reactive] public bool RunningObservable { get; private set; } = false;
        [Reactive] public bool NumbersChanged { get; private set; } = false;
        public ReadOnlyObservableCollection<int> Numbers => _numbers;
        private readonly ReadOnlyObservableCollection<int> _numbers;
        private readonly ReactiveCommand<Unit, Unit> _loadCommand;
        private IDisposable _cancel;
        private readonly IDataService _dataService;
        private readonly IDisposable _disposeBinding;
        private IDisposable _disposeCommand;

        public ReactiveViewModel(IServiceScopeFactory scopeFactory)
        {
            using (var serviceScope = scopeFactory.CreateScope())
            {
                _dataService = serviceScope.ServiceProvider.GetService<IDataService>();
            }

            _loadCommand = ReactiveCommand.CreateFromTask(ct => _dataService.CreateData(ct));
            _loadCommand.IsExecuting.ToPropertyEx(this, x => x.Running);

            _disposeBinding = _dataService.DataChangeSet
                   .Bind(out _numbers)
                   .Subscribe();
        }

        public void Dispose()
        {
            _disposeBinding.Dispose();
        }

        public void Start()
        {
            if (Running)
                Cancel();

            _cancel = _loadCommand.Execute().Subscribe();
        }
        public void Cancel()
        {
            _cancel?.Dispose();
            _dataService?.ClearData();
        }

        public void StartObservable()
        {
            RunningObservable = true;
            _disposeCommand = _dataService.CreateDataFromInterval();
        }
        public void CancelObservable()
        {
            _disposeCommand?.Dispose();
            _dataService?.ClearData();
            RunningObservable = false;
        }
    }
}