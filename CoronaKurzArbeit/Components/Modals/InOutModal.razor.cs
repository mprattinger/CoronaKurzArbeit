using Blazored.Modal;
using Blazored.Modal.Services;
using CoronaKurzArbeit.Models;
using CoronaKurzArbeit.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaKurzArbeit.Components.Modals
{

    [Obsolete]
    public partial class InOutModal
    {

        [Inject]
        protected TimeRegistrationService TimeRegistrationService { get; set; } = default!;

        [CascadingParameter]
        public BlazoredModalInstance? BlazoredModal { get; set; }

        [Parameter]
        public InOutType IOType { get; set; } = InOutType.UNKNOWN;

        public string TimeLabel { get; set; } = string.Empty;

        public DateTime Time { get; set; } = DateTime.Now;

        protected override void OnInitialized()
        {
            //BlazoredModal.SetTitle("Enter a Message");
            switch (IOType)
            {
                case InOutType.UNKNOWN:
                    break;
                case InOutType.IN:
                    BlazoredModal?.SetTitle("Wann bist du in die Firma gekommen?");
                    TimeLabel = "Gekommen:";
                    break;
                case InOutType.OUT:
                    BlazoredModal?.SetTitle("Wann hast du die Firma verlassen?");
                    TimeLabel = "Gegangen:";
                    break;
                case InOutType.PIN:
                    BlazoredModal?.SetTitle("Wann hast du mit der Pause begonnen?");
                    TimeLabel = "Pause Beginn:";
                    break;
                case InOutType.POUT:
                    BlazoredModal?.SetTitle("Wann bist du von der Pause zurück gekommen?");
                    TimeLabel = "Pause Ende:";
                    break;
                default:
                    break;
            }
        }

        public async Task SubmitForm()
        {
            var trm = new TimeRegistration
            {
                RegistrationMoment = Time,
                Type = IOType
            };
            await TimeRegistrationService.AddRegistration(trm);
            BlazoredModal?.Close();
        }

        public void Cancel()
        {
            BlazoredModal?.Cancel();
        }
    }
}
