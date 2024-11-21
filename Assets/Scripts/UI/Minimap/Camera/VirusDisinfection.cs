//using UnityEngine;

//public class VirusDisinfection : MonoBehaviour
//{
//    private LayerMask mainCameraCullingMask; // Main 카메라의 CullingMask
//    private OneClearManager oneClearManager;

//    void Start()
//    {
//        // Main 카메라의 CullingMask 설정
//        mainCameraCullingMask = Camera.main.cullingMask;
//        oneClearManager = FindObjectOfType<OneClearManager>();
//    }

//    void FixedUpdate()
//    {
//        // 왼쪽 마우스 버튼이 눌렸을 때 처리
//        if (Input.GetMouseButtonDown(0))
//        {
//            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 마우스 포인터의 화면 좌표를 기준으로 Ray 생성
//            RaycastHit hit; // RaycastHit 정보 저장 변수

//            // 충돌을 감지하도록 설정, Main 카메라의 CullingMask를 사용하여 검사
//            if (Physics.Raycast(ray, out hit, 100f, mainCameraCullingMask))
//            {
//                Virus virus = hit.collider.GetComponentInParent<Virus>(); // Raycast로 hit한 오브젝트나 부모에서 Virus 컴포넌트 찾기

//                if (virus != null)
//                {
//                    if (oneClearManager != null && OneClearManager.Instance.isDisinfectionOn)
//                    {
//                        virus.Disinfect(); // 소독 실행
//                    }
//                    else
//                    {
//                        Debug.Log("소독이 비활성화되어 바이러스가 삭제되지 않았습니다.");
//                    }
//                }
//            }
//            else
//            {
//                Debug.Log("Raycast did not hit any object."); // Raycast가 아무 오브젝트도 hit하지 못한 경우 로그 출력
//            }
//        }
//    }
//}
