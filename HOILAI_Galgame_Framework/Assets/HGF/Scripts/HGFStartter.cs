using UnityEngine;

namespace HGF
{
    public class HGFStartter : MonoBehaviour
    {
        private void Start ()
        {
            var _galMananger = gameObject.GetComponent<GalManager>();
            var _gameModel = new HGFStartModel();

            _gameModel.characterInfo = Resources.Load<TextAsset>("HGF/Test_Character").text;
            _gameModel.departmentInfo = Resources.Load<TextAsset>("HGF/Test_Department").text;
            _gameModel.plotText = Resources.Load<TextAsset>("HGF/Test_Plot").text;

            _galMananger.StartGame(_gameModel);
        }
    }
}
