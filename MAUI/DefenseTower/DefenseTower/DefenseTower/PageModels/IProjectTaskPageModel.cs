using CommunityToolkit.Mvvm.Input;
using DefenseTower.Models;

namespace DefenseTower.PageModels;

public interface IProjectTaskPageModel
{
	IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
	bool IsBusy { get; }
}