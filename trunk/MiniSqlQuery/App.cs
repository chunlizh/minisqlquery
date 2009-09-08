﻿using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using MiniSqlQuery.Core;
using MiniSqlQuery.Core.DbModel;
using MiniSqlQuery.Core.Template;
using MiniSqlQuery.PlugIns;
using MiniSqlQuery.PlugIns.ConnectionStringsManager;
using MiniSqlQuery.PlugIns.DatabaseInspector;
using MiniSqlQuery.PlugIns.SearchTools;
using MiniSqlQuery.PlugIns.TemplateViewer;
using MiniSqlQuery.PlugIns.ViewTable;
using MiniSqlQuery.Properties;

namespace MiniSqlQuery
{
	internal static class App
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
#if !DEBUG
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
			Application.ThreadException += ApplicationThreadException;
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

			IApplicationServices services = ApplicationServices.Instance;

			ConfigureContainer(services);

			services.LoadPlugIn(new CoreApplicationPlugIn());
			services.LoadPlugIn(new ConnectionStringsManagerLoader());
			services.LoadPlugIn(new DatabaseInspectorLoader());
			services.LoadPlugIn(new ViewTableLoader());
			services.LoadPlugIn(new TemplateViewerLoader());
			services.LoadPlugIn(new SearchToolsLoader());

			IPlugIn[] plugins = PlugInUtility.GetInstances<IPlugIn>(Environment.CurrentDirectory, Settings.Default.PlugInFileFilter);
			Array.Sort(plugins, new PlugInComparer());
			foreach (IPlugIn plugin in plugins)
			{
				services.LoadPlugIn(plugin);
			}

			services.HostWindow.SetArguements(args);
			Application.Run(services.HostWindow.Instance);
		}

		public static void ConfigureContainer(IApplicationServices services)
		{
			// singletons
			services.RegisterSingletonComponent<IApplicationSettings, ApplicationSettings>("ApplicationSettings");
			services.RegisterSingletonComponent<IHostWindow, MainForm>("HostWindow");
			services.RegisterSingletonComponent<IFileEditorResolver, FileEditorResolverService>("FileEditorResolver");

			// components
			services.RegisterComponent<AboutForm>("AboutForm");
			services.RegisterComponent<ITextFindService, BasicTextFindService>("DefaultTextFindService");
			services.RegisterComponent<IQueryEditor, QueryForm>("QueryForm");
			services.RegisterComponent<ISqlWriter, SqlWriter>("DefaultSqlWriter");
			services.RegisterComponent<ITextFormatter, NVelocityWrapper>("TextFormatter");
			services.RegisterComponent<TemplateModel>("TemplateModel");
		}

		private static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
		{
			if (!(e.Exception is ThreadAbortException))
			{
				HandleException(e.Exception);
			}
		}

		private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (!(e.ExceptionObject is ThreadAbortException))
			{
				HandleException((Exception) e.ExceptionObject);
			}
		}

		private static void HandleException(Exception e)
		{
			ErrorForm errorForm = new ErrorForm();
			errorForm.SetException(e);
			errorForm.ShowDialog();
			errorForm.Dispose();
		}
	}
}