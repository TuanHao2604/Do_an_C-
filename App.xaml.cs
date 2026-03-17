using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using TravelGuideApp.Database;

namespace TravelGuideApp;

public partial class App : Application
{
	public App(SQLiteService database)
	{
		InitializeComponent();

		MainPage = new AppShell();

		Task.Run(async () =>
		{
			try
			{
				await database.SeedDataAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Seed data failed: {ex.Message}");
			}
		});

			// Notification permission request can be ha~~ndled by platform-specific code if needed.
	}
}
