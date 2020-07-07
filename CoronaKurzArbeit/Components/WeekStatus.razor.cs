using CoronaKurzArbeit.Services;
using CoronaKurzArbeit.Shared.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Components
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public partial class WeekStatus : IDisposable
    {
        [Inject]
        public IAppState AppState { get; set; } = default!;



        public InfoViewModel2 InfoData { get; set; } = new InfoViewModel2();

        protected override void OnInitialized()
        {
            AppState.OnDayTimeListLoadedFinished += appState_OnDayTimeListLoadedFinished;
        }

        private Task appState_OnDayTimeListLoadedFinished(DateTime arg)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            AppState.OnCurrentDayChanged -= appState_OnDayTimeListLoadedFinished;
        }
    }
}
