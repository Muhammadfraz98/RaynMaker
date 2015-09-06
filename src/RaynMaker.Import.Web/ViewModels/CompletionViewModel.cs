﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Blade.Data;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using RaynMaker.Import.Html.WinForms;
using RaynMaker.Import.Spec;
using RaynMaker.Import.Web.Model;

namespace RaynMaker.Import.Web.ViewModels
{
    class CompletionViewModel
    {
        private Session mySession;

        public CompletionViewModel( Session session )
        {
            mySession = session;

            ValidateCommand = new DelegateCommand( OnValidate );
            ClearCommand = new DelegateCommand( OnClear );
            SaveCommand = new DelegateCommand( OnSave );

            InputMacroValueRequest = new InteractionRequest<IConfirmation>();
        }

        public IBrowser Browser { get; set; }

        public ICommand ValidateCommand { get; private set; }

        private void OnValidate()
        {
            if( mySession.CurrentSite == null )
            {
                return;
            }

            var macroPattern = new Regex( @"(\$\{.*\})" );
            var filtered = new List<NavigatorUrl>();
            foreach( var navUrl in mySession.CurrentSite.Navigation.Uris )
            {
                var md = macroPattern.Match( navUrl.UrlString );
                if( md.Success )
                {
                    string macro = md.Groups[ 1 ].Value;
                    string value = GetValue( macro );
                    if( value != null )
                    {
                        filtered.Add( new NavigatorUrl( navUrl.UrlType, navUrl.UrlString.Replace( macro, value ) ) );
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    filtered.Add( navUrl );
                }
            }

            var doc = Browser.LoadDocument( filtered );

            var markupDoc = new MarkupDocument();
            markupDoc.Document = ( ( HtmlDocumentAdapter )doc ).Document;
            markupDoc.Anchor = mySession.CurrentFormat.Path;
            markupDoc.Dimension = mySession.CurrentFormat.Expand;
            markupDoc.SeriesName = mySession.CurrentFormat.SeriesName;
            if( mySession.CurrentFormat.Expand == CellDimension.Row )
            {
                markupDoc.ColumnHeaderRow = mySession.CurrentFormat.TimeAxisPosition;
                markupDoc.RowHeaderColumn = mySession.CurrentFormat.SeriesNamePosition;
            }
            else if( mySession.CurrentFormat.Expand == CellDimension.Column )
            {
                markupDoc.RowHeaderColumn = mySession.CurrentFormat.TimeAxisPosition;
                markupDoc.ColumnHeaderRow = mySession.CurrentFormat.SeriesNamePosition;
            }
            markupDoc.SkipColumns = mySession.CurrentFormat.SkipColumns;
            markupDoc.SkipRows = mySession.CurrentFormat.SkipRows;

            markupDoc.Apply();
        }

        private string GetValue( string macro )
        {
            var notification = new Confirmation();
            notification.Title = "Enter macro value";
            notification.Content = macro;

            string result = null;

            InputMacroValueRequest.Raise( notification, c =>
            {
                if( c.Confirmed )
                {
                    result = ( string )c.Content;
                }
            } );

            return result;
        }

        public InteractionRequest<IConfirmation> InputMacroValueRequest { get; private set; }

        public ICommand ClearCommand { get; private set; }

        private void OnClear()
        {
            mySession.Reset();
        }

        public ICommand SaveCommand { get; private set; }

        private void OnSave()
        {

        }
    }
}
