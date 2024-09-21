using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }

    private List<string> ids = new List<string>();
    private List<string> usernames = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            AddDefaultUser(); // 기본 사용자 정보 추가
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 기본 유저 추가 (더미)
    private void AddDefaultUser()
    {
        ids.Add("1");
        usernames.Add("Q");
    }

    // 회원가입 통해 정보 추가
    public void AddUser(string id, string username)
    {
        ids.Add(id);
        usernames.Add(username);
    }

    // 로그인 정보 확인 (둘다 일치 시 true)
    public bool ValidateUser(string id, string username)
    {
        int index = ids.IndexOf(id);
        return index != -1 && usernames[index] == username;
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
        return ids.Contains(id);
    }

    // 이름 존재 유무
    public bool IsUsernameExists(string username)
    {
        return usernames.Contains(username);
    }
}
