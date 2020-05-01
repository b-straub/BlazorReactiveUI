using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using ReactiveUI;

namespace BlazorReactiveUI.Client.Reactive
{
    public class ReactiveOwningComponentBase<T> : OwningComponentBase where T: ReactiveObject
    {
        public T ViewModel { get; private set; }

        private IDisposable _disposeObservable;
        private IObservable<Unit> _renderObservable;

        protected override void OnInitialized()
        {
            ViewModel = (T)ScopedServices.GetService(typeof(T));

            _renderObservable = ViewModel.Changed.Select(cev => Unit.Default);
            _disposeObservable = _renderObservable.Subscribe(_ =>
                InvokeAsync(StateHasChanged)
            );

            base.OnInitialized();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposeObservable?.Dispose();
            }

            base.Dispose(disposing);
        }

        public void AddRenderObservable(IObservable<Unit> renderObservable)
        {
            _disposeObservable?.Dispose();
            _renderObservable = Observable.Merge(_renderObservable, renderObservable);
            _disposeObservable = _renderObservable.Subscribe(_ =>
                InvokeAsync(StateHasChanged)
            );
        }
    }
}
