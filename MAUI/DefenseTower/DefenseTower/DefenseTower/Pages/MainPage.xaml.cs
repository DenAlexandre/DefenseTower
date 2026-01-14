using DefenseTower.Models;
using DefenseTower.PageModels;

namespace DefenseTower.Pages;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}