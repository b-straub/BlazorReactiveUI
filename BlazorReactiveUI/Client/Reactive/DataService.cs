using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;

namespace BlazorReactiveUI.Client.Reactive
{
    public interface IDataService
    {
        public IObservable<IChangeSet<int>> DataChangeSet { get; }
        public Task CreateData(CancellationToken ct);
        public IDisposable CreateDataFromInterval();
        public void ClearData();
    }

    public class DataService : IDataService
    {
        public IObservable<IChangeSet<int>> DataChangeSet => _dataChangeSet;

        private SourceList<int> _dataList = new SourceList<int>();
        private IObservable<IChangeSet<int>> _dataChangeSet;

        public DataService()
        {
            _dataChangeSet = _dataList.Connect();
        }

        public void ClearData()
        {
            _dataList.Clear();
        }

        public async Task CreateData(CancellationToken ct)
        {
            var rng = new Random();

            while (!ct.IsCancellationRequested)
            {
                _dataList.Edit(innerList =>
                {
                    innerList.Clear();

                    Observable.Range(0, 20).Subscribe(index =>
                    {
                        var number = rng.Next(-10000, 10000);
                        innerList.Add(number);
                    });
                });
                await Task.Yield();
            }
        }

        public IDisposable CreateDataFromInterval()
        {
            var rng = new Random();

            return Observable.Interval(TimeSpan.FromMilliseconds(10))
                .Subscribe(_ =>
                {
                    _dataList.Edit(innerList =>
                    {
                        innerList.Clear();

                        Observable.Range(0, 20).Subscribe(index =>
                        {
                            var number = rng.Next(-10000, 10000);
                            innerList.Add(number);
                        });
                    });
                });
        }
    }
}
