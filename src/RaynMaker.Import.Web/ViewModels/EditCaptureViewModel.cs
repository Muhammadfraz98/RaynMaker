﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.Collections;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.Windows.Controls;
using RaynMaker.Import.Spec;

namespace RaynMaker.Import.Web.ViewModels
{
    [Export]
    class EditCaptureViewModel : BindableBase, IInteractionRequestAware
    {
        private NavigationUrl mySelectedUrl;
        private INotification myNotification;

        public EditCaptureViewModel()
        {
            Urls = new ObservableCollection<NavigationUrl>();

            DeleteUrlCommand = new DelegateCommand<NavigationUrl>( OnDeleteUrl );

            OkCommand = new DelegateCommand( OnOk );
            CancelCommand = new DelegateCommand( OnCancel );
        }

        public ObservableCollection<NavigationUrl> Urls { get; private set; }

        public NavigationUrl SelectedUrl
        {
            get { return mySelectedUrl; }
            set { SetProperty( ref mySelectedUrl, value ); }
        }

        public ICommand DeleteUrlCommand { get; private set; }

        private void OnDeleteUrl( NavigationUrl url )
        {
            Urls.Remove( url );
        }

        public ICommand OkCommand { get; private set; }

        private void OnOk()
        {
            TextBoxBinding.ForceSourceUpdate();

            Notification.TrySetConfirmed( true );

            Notification.Content = Urls.ToList();

            FinishInteraction();
        }

        public ICommand CancelCommand { get; private set; }

        private void OnCancel()
        {
            Notification.TrySetConfirmed( false );
            FinishInteraction();
        }

        public Action FinishInteraction { get; set; }

        public INotification Notification
        {
            get { return myNotification; }
            set
            {
                myNotification = value;

                Urls.Clear();
                Urls.AddRange( ( IEnumerable<NavigationUrl> )myNotification.Content );
                SelectedUrl = Urls.FirstOrDefault();
            }
        }
    }
}
