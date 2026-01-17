using DefenseTowerOrigin.Models;
using DefenseTowerOrigin.PageModels;

namespace DefenseTowerOrigin.Pages;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}