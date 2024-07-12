namespace ADMail.Pages
{
    /// <summary>
    /// Interaktionslogik für ContentPage.xaml
    /// </summary>
    public partial class ContentPage
    {
        internal static ContentPage? ContentWindow;
        internal static UserList? UserListPage;

        public ContentPage()
        {
            InitializeComponent();
            ContentWindow = this;

            UserListPage = new UserList();
            FrameWindow.Content = UserListPage;
        }
    }
}
