using Caliburn.Micro;
using RadioOwl.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Squirrel;

namespace RadioOwl
{
    /// <summary>
    /// Bostrapper pro caliburn micro aplikaci
    /// </summary>
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            // sileny zpusob jak v caliburnu nastavit iconu formulare ;-)
            // http://stackoverflow.com/questions/27227892/how-do-i-set-a-window-application-icon-in-a-application-set-up-with-caliburn-mic
            var settings = new Dictionary<string, object>
            {
                // ikona musi byt jako Resources, ne EmbeddedResources!
                // format pack url viz:
                // https://docs.microsoft.com/en-us/dotnet/desktop/wpf/app-development/pack-uris-in-wpf?redirectedfrom=MSDN&view=netframeworkdesktop-4.8
                // https://stackoverflow.com/questions/16409819/pack-uri-to-image-embedded-in-a-resx-file
                { "Icon", new BitmapImage(new Uri("pack://application:,,,/RadioOwl;component/Icons/1477096338_owl.png")) },
            };
            // spusteni view - konkretni view se dohledava pres jmeno, je tedy nutne dodrzovat konvenci jmenoView a jmenoViewModel
            DisplayRootViewFor<MainViewModel>(settings);

            // test formulare
            //DisplayRootViewFor<Forms.Test.Window1ViewModel>(settings);
            //DisplayRootViewFor<Forms.Test.Window2ViewModel>(settings);

            CheckForUpdates();
        }

        private void CheckForUpdates()
        {
            //using var upManager = new Squirrel. UpdateManager();
        }
    }
}
