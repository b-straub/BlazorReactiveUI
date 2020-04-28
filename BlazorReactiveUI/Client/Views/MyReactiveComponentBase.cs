// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace ReactiveUI.Blazor
{
    /// <summary>
    /// A base component for handling property changes and updating the blazer view appropriately.
    /// </summary>
    /// <typeparam name="T">The type of view model. Must support INotifyPropertyChanged.</typeparam>
    public class MyReactiveComponentBase<T> : ComponentBase, IViewFor<T>, INotifyPropertyChanged, ICanActivate, IDisposable
        where T : ReactiveObject
    {
        private readonly Subject<Unit> _initSubject = new Subject<Unit>();
        [SuppressMessage("Design", "CA2213: Dispose object", Justification = "Used for deactivation.")]
        private readonly Subject<Unit> _deactivateSubject = new Subject<Unit>();

        private T _viewModel;

        private bool _disposedValue; // To detect redundant calls
        private IDisposable _disposeStateChanged; // Disposable for state changed subscription 

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        [Parameter]
        public T ViewModel
        {
            get => _viewModel;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_viewModel, value))
                {
                    return;
                }

                _viewModel = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (T)value;
        }

        /// <inheritdoc />
        public IObservable<Unit> Activated => _initSubject.AsObservable();

        /// <inheritdoc />
        public IObservable<Unit> Deactivated => _deactivateSubject.AsObservable();

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) below.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            _initSubject.OnNext(Unit.Default);
            base.OnInitialized();
        }

        /// <inheritdoc/>
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                // The following subscriptions are here because if they are done in OnInitialized, they conflict with certain JavaScript frameworks.
                
                // Observable for ViewModel assignement
                var viewModelChanged =
                    this.WhenAnyValue(x => x.ViewModel)
                        .Where(x => x != null)
                        .Select(x => Unit.Default);

                // Observable for any property changed notification
                var propertyChanged =
                    this.WhenAnyValue(x => x.ViewModel)
                    .Where(x => x != null)
                    .Select(x => x.Changed)
                    .Switch()
                    .Select(x => Unit.Default);

                // combined observable with a 50ms throttle to avoid to frequent rerendering
                var combinedChanged = Observable.Merge(viewModelChanged, propertyChanged).Throttle(TimeSpan.FromMilliseconds(50));

                _disposeStateChanged = combinedChanged.Subscribe(x =>
                    InvokeAsync(StateHasChanged)
                );
            }

            base.OnAfterRender(firstRender);
        }

        /// <summary>
        /// Invokes the property changed event.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Cleans up the managed resources of the object.
        /// </summary>
        /// <param name="disposing">If it is getting called by the Dispose() method rather than a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _initSubject?.Dispose();
                    _disposeStateChanged?.Dispose();
                    _deactivateSubject.OnNext(Unit.Default);
                }

                _disposedValue = true;
            }
        }
    }
}