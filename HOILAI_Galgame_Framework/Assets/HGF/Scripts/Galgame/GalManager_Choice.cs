using UnityEngine;

namespace HGF
{
    public class GalManager_Choice : MonoBehaviour
    {
        private GameObject GameObject_Choice;

        private void Start ()
        {
            GameObject_Choice = Resources.Load<GameObject>("HGF/Button-Choice");
        }
        [SerializeField]
        public void CreatNewChoice (string JumpID, string Title)
        {

            var _ = GameObject_Choice;
            _.GetComponent<GalComponent_Choice>().Init(JumpID, Title);
            Instantiate(_, transform);
            return;
        }
        public void Button_Click_Choice ()
        {


            for (int i = 0; i < transform.childCount; i++)
            {
                //不可用DestroyImmediate
                //原因：DestroyImmediate是同步的，如果使用则会导致每次获取的都是0，无法删除，
                Destroy(transform.GetChild(i).gameObject);
            }
            return;
        }
    }
}