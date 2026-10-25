using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Backgammon.Client.Controllers.Profile;
using Backgammon.Client.Controllers.Session;

namespace Backgammon.Client.Views.Pages
{
    public partial class PlayerProfile : Page
    {
        private readonly ProfileController _profileController;

        public PlayerProfile()
        {
            InitializeComponent();
            _profileController = new ProfileController();

            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentSession = PlayerSessionContext.GetSession();

                Backgammon.Client.ProfileReference.PlayerProfile profileData =
                    await _profileController.GetPlayerProfileAsync(currentSession.SessionId);

                if (profileData != null)
                {
                    textBlockPoints.Text = profileData.Points.ToString();
                    textBlockWins.Text = profileData.Wins.ToString();

                    if (string.IsNullOrWhiteSpace(profileData.Description))
                    {
                        textBlockDescription.Text = "Sin descripción...";
                    }
                    else
                    {
                        textBlockDescription.Text = profileData.Description;
                    }

                    LoadSocialMediaLinks(profileData);

                    ellipseAvatar.Fill = (Brush)FindResource("CyanAccentBrush");
                }
                else
                {
                    MessageBox.Show("No se pudo cargar la información del perfil. Verifica tu conexión.",
                                    "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error de Sesión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSocialMediaLinks(Backgammon.Client.ProfileReference.PlayerProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.FacebookLink))
            {
                textBlockFacebook.Text = "No enlazado";
            }
            else
            {
                textBlockFacebook.Text = profile.FacebookLink;
            }

            if (string.IsNullOrWhiteSpace(profile.InstagramLink))
            {
                textBlockInstagram.Text = "No enlazado";
            }
            else
            {
                textBlockInstagram.Text = profile.InstagramLink;
            }

            if (string.IsNullOrWhiteSpace(profile.TikTokLink))
            {
                textBlockTikTok.Text = "No enlazado";
            }
            else
            {
                textBlockTikTok.Text = profile.TikTokLink;
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void OnAddFacebookClick(object sender, RoutedEventArgs e)
        {
            // TODO : Implementar la funcionalidad para agregar Facebook
            MessageBox.Show("Funcionalidad para agregar Facebook en desarrollo.");
        }

        private void OnAddInstagramClick(object sender, RoutedEventArgs e)
        {
            // TODO : Implementar la funcionalidad para agregar Instagram
            MessageBox.Show("Funcionalidad para agregar Instagram en desarrollo.");
        }

        private void OnAddTikTokClick(object sender, RoutedEventArgs e)
        {
            // TODO : Implementar la funcionalidad para agregar TikTok
            MessageBox.Show("Funcionalidad para agregar TikTok en desarrollo.");
        }
    }
}