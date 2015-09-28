
using PetraERP.Shared.UI;
using System.Collections.Generic;
using System.ComponentModel;

namespace PetraERP.Shared.UI.Navigation
{
    public interface INavigator:INotifyPropertyChanged
    {
        void NavigateBack();

        void NavigateToHome();

        WorkspaceViewModelBase CurrentView { get; set; }

        IEnumerable<WorkspaceViewModelBase> GetAllView();

        void AddView(WorkspaceViewModelBase workspaceView);

        void AddHomeView(WorkspaceViewModelBase workspaceView);

        void NavigateToView(string viewKey);
    }
}
