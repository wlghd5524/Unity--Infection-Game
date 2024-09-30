using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using MyApp.DataAccess;
using System;
using System.Collections;
using UnityEditor.PackageManager.Requests;
using UnityEngine.Networking;

namespace MyApp.UserManagement
{
    [System.Serializable]
    public class User
    {
        public int nurse_id;
        public string nurse_name;
        public int tutorial;
    }

    public class UserManager : MonoBehaviour
    {
        public static UserManager Instance { get; private set; }

        private string urlGetLogin = "http://220.69.209.164:3333/get_login"; // Flask 서버 IP

        private List<User> users;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Start()
        {
            users = new List<User>();
            StartCoroutine(GetUsersData());
        }

        // Flask API에서 사용자 데이터 가져오기
        IEnumerator GetUsersData()
        {
            UnityWebRequest www = UnityWebRequest.Get(urlGetLogin);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching user data: " + www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("User Login JSON Response: " + jsonResponse);

                try
                {
                    List<User> fetchedUsers = MyApp.Utilities.JsonHelper.FromJson<User>(jsonResponse);
                    if (fetchedUsers != null)
                    {
                        users = fetchedUsers;
                        Debug.Log($"Successfully fetched {users.Count} users.");
                    }
                    else
                    {
                        Debug.LogError("Failed to parse user data, response was: " + jsonResponse);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing JSON: " + e.Message + ". JSON Response: " + jsonResponse);
                }
            }
        }

        // 로그인 정보 확인 (둘다 일치 시 true)
        public bool ValidateUser(string id, string name)
        {
            foreach (var user in users)
            {
                if (user.nurse_id.ToString() == id && user.nurse_name == name)
                {
                    return true;
                }
            }
            Debug.LogError("로그인DB에서 사용자 정보 찾기 실패");
            return false;
        }

        // 유저의 튜토리얼 진행 여부 확인
        public int GetUserTutorialStatus(string id)
        {
            foreach (var user in users)
            {
                if (user.nurse_id.ToString() == id)
                {
                    return user.tutorial;
                }
            }
            return -1;  // 유저를 찾지 못했을 때
        }

        // 사원 번호 유효성 검사 (숫자만 허용)
        public bool IsValidID(string id)
        {
            return Regex.IsMatch(id, @"^[0-9]+$");
        }

        // 사용자 이름 유효성 검사 (문자만 허용)
        public bool IsValidUsername(string username)
        {
            return Regex.IsMatch(username, @"^[a-zA-Z가-힣]+$");
        }

        // ID 존재 유무
        public bool IsIDExists(string id)
        {
            foreach (var user in users)
            {
                if (user.nurse_id.ToString() == id)
                {
                    return true;
                }
            }
            return false;
        }

        // 이름 존재 유무
        public bool IsUsernameExists(string username)
        {
            foreach (var user in users)
            {
                if (user.nurse_name == username)
                {
                    return true;
                }
            }
            return false;
        }

        // 회원가입을 통해 로그인DB에 회원 정보 추가
        // 튜토리얼 진행 여부 업데이트
        public void AddUser(string id, string username, int istutorial)
        {
            WWWForm form = new WWWForm();
            form.AddField("nurse_id", id);
            form.AddField("nurse_name", username);
            form.AddField("tutorial", istutorial);

            Debug.Log($"DB: Sending Data: nurse_id={id}, nurse_name={username}");

            string urlSignupData = "http://220.69.209.164:3333/signup_or_update_tutorial";

            StartCoroutine(PostRequest(urlSignupData, form));
        }

        // Send a POST request to the Flask API
        IEnumerator PostRequest(string url, WWWForm form)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error sending request: " + www.error);
                }
                else
                {
                    Debug.Log("Request successfully sent: " + www.downloadHandler.text);

                    // 회원가입 성공 시 새로고침
                    StartCoroutine(GetUsersData());
                }
            }
        }
    }
}
