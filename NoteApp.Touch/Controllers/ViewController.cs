﻿using System;

using UIKit;
using Autofac;
using NoteApp.Core.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using NoteApp.Core.BusinessObjects;
using NoteApp.Touch.Helpers;
using CoreGraphics;
using NoteApp.Touch.Controllers;

namespace NoteApp.Touch
{
	public partial class ViewController : BaseViewController
	{
		private NoteViewModel _viewModel;
		private UITableView _notesTableView;
		private UIActivityIndicatorView _dialog;
		private UIRefreshControl _refresher;
		private NotesTableSource _noteSource;

		public ViewController () : base ()
		{
			using (var scope = App.Container.BeginLifetimeScope ()) {
				_viewModel = scope.Resolve<NoteViewModel> ();
			}
		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			_dialog = DialogHelper.ShowProgressDialog (View.Frame, View);

			Title = "My Notes";
			View.BackgroundColor = UIColor.White;

			NavigationController.NavigationBar.Translucent = false;

			InitUI ();
		}

		private async void InitUI ()
		{
			nfloat startY = GetStatusBarHeight ();

			_notesTableView = new UITableView (new CGRect (0, 0, View.Frame.Width, View.Frame.Height - startY));
			_notesTableView.RowHeight = 70;

			this.NavigationItem.SetRightBarButtonItem (
				new UIBarButtonItem (UIBarButtonSystemItem.Add, (sender, args) => {
					NavigationController.PushViewController (new AddNoteViewController (_viewModel), false);
				})
				, true);


			_refresher = new UIRefreshControl ();
			_refresher.ValueChanged += Refresh;

			_notesTableView.AddSubview (_refresher);
			View.AddSubview (_notesTableView);
			DialogHelper.DismissProgressDialog (_dialog);
		}

		public async override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			List<Note> notes = null;

			await Task.Run (() => {
				notes = _viewModel.GetNotes ();
			});

			_noteSource = new NotesTableSource (notes, View.Frame.Width, 70);
			_notesTableView.DataSource = _noteSource;
			_notesTableView.ReloadData ();

		}

		void Refresh (object sender, EventArgs e)
		{
			Task.Run (() => {
				var notes = _viewModel.GetNotes ();
				if (notes != null) {
					InvokeOnMainThread (() => {
						_noteSource.SDSource = notes;
						_notesTableView.ReloadData ();
						_refresher.EndRefreshing ();
					});
				}
			});
		}


	}
}

