using System;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using DynamicData.Binding;
using BlazorReactiveUI.Client.ViewModels;
using BlazorReactiveUI.Client.Reactive;

namespace BlazorReactiveUI.Client.Views
{
    public partial class ReactiveTest : ReactiveOwningComponentBase<ReactiveViewModel>
    {
        public ReactiveTest()
        {
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                AddRenderObservable(ViewModel.Numbers.ObserveCollectionChanges().Select(x => Unit.Default));
            }

            base.OnAfterRender(firstRender);
        }

        /*protected override void Dispose(bool disposing)
         {
             if (disposing)
             {
                 _disposeBag.Dispose();
             }
             base.Dispose(disposing);
         }*/
    }
}
