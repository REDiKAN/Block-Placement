using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;

namespace Game.Views.UI
{
    public partial class PauseMenuView
    {
        private void ReturnToMenu()
        {
            Time.timeScale = 1f;
            LevelContext.SelectedLevelId = 0;
            LevelContext.SelectedCategoryId = 0;
            SceneManager.LoadScene("MenuScene");
        }
    }
}