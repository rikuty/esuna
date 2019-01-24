using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


namespace Gamestrap
{
    public class MainMenuControl : UtilComponent
    {
        private static int visibleVariable = Animator.StringToHash("Visible");
        private static int notifyVariable = Animator.StringToHash("Notify");

        public GameObject settingsPanel, aboutPanel;

        public Toggle soundToggle, musicToggle;

        private string userID;
        private string userName;
        public DateTime dateTime;
        public Text txtID;
        public Text txtName;

        public GazeButtonInput gazeButtonInput;

        private GameData gameData;

        public Text notificationText;
        private Animator notificationAnimator;
        public void Start()
        {
            //Adds events to the Toggle buttons through code since
            //doing it through the inspector wouldn't will give the value of the button dynamically
            soundToggle.onValueChanged.AddListener(ToggleSound);
            musicToggle.onValueChanged.AddListener(ToggleMusic);

            this.gameData = GameData.Instance;

            notificationAnimator = notificationText.GetComponent<Animator>();

            this.dateTime = DateTime.Now;
            //ID = System.DateTime.Now.ToString("yyMMddHHmm");
            //this.userID = this.dateTime.ToString("yyMMddHHmm");

            // Get user data
            string targetAPIURL = "http://rikuty.main.jp/test/GetUserData.php";
			StartCoroutine (ConnectAPI(targetAPIURL, this.SuccessCallbackAPI));

            //this.gazeButtonInput.Init(this.context);


            this.PlayMain();
        }

		private void SuccessCallbackAPI(string wwwText){
			UserData userData = JsonUtility.FromJson<UserData>(wwwText);

            SetLabel(this.txtID, userData.user_id);
            SetLabel(this.txtName, userData.user_name);
		}

        #region Event Methods Called from the UI

        public RawImage imgMain;

        private ESceneNames name = ESceneNames.Main;

        public void PlayClick()
        {
            GSAppExampleControl.Instance.LoadScene(this.name);
        }


        public void PlayMain()
        {
            this.name = ESceneNames.Main;
            this.imgMain.color = new Color(this.imgMain.color.r, this.imgMain.color.g, this.imgMain.color.b, 255f);

        }

        public void ClickTrigger()
        {
        }

        private void Update()
        {
            bool result = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger);
            if (result)
                PlayClick();
        }

        public void AchievementsClick()
        {
            notificationText.text = "Achievements Clicked...";
            notificationAnimator.SetTrigger(notifyVariable);
        }

        public void LeaderboardClick()
        {
            notificationText.text = "Leaderboard Clicked...";
            notificationAnimator.SetTrigger(notifyVariable);
        }

        public void RateClick()
        {
            notificationText.text = "Rate Clicked...";
            notificationAnimator.SetTrigger(notifyVariable);
        }

        #region Settings Events
        public void ToggleSettingsPanel()
        {
            TogglePanel(settingsPanel.GetComponent<Animator>());
        }

        public void ToggleSound(bool on)
        {
            // Change the sound
        }

        public void ToggleMusic(bool on)
        {
            // Change the music
        }

        #endregion

        #region About Events

        public void FacebookClick()
        {
            Application.OpenURL("https://www.facebook.com/gamestrapui/");
        }

        public void TwitterClick()
        {
            Application.OpenURL("https://twitter.com/EmeralDigEnt");

        }

        public void YoutubeClick()
        {
            Application.OpenURL("https://www.youtube.com/channel/UC8b_9eMveC6W0hl5RJkCvyQ");
        }

        public void WebsiteClick()
        {
            Application.OpenURL("http://www.gamestrap.info");
        }
        #endregion

        public void ToggleAboutPanel()
        {
            TogglePanel(aboutPanel.GetComponent<Animator>());
        }

        private void TogglePanel(Animator panelAnimator)
        {
            panelAnimator.SetBool(visibleVariable, !panelAnimator.GetBool(visibleVariable));
        }
        #endregion
    }
}