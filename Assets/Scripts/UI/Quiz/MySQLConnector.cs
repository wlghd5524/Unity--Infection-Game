using UnityEngine;
using System.Text;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class User
{
    public string quest;
    public string answer1;
    public string answer2;
    public string answer3;
    public string answer4;
    public int right;
}

public class MySQLConnector : MonoBehaviour
{
    private string url1 = "http://220.69.209.170:3333/get_levels";
    private string url2 = "http://220.69.209.170:3333/get_answers";

    private List<User> users;
    private List<User> answers;

    public void Start()
    {
        users = new List<User>();
        answers = new List<User>();

        StartCoroutine(GetUsersData((result) => { users = result; }));
        StartCoroutine(GetAnswerData((result) => { answers = result; }));
    }

    public List<User> GetUsers() => users;
    public List<User> GetAnswers() => answers;

    IEnumerator GetUsersData(Action<List<User>> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(url1);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            byte[] result = www.downloadHandler.data;
            string jsonResponse = Encoding.UTF8.GetString(result);
            //Debug.Log("JSON Response: " + jsonResponse); // JSON 응답 확인
            List<User> users = JsonHelper.FromJson<User>(jsonResponse);
            if (users != null)
            {
                callback(users);
            }
            /*foreach (User user in users)
            {
                Debug.Log("Quest: " + user.quest + ", 1. " + user.answer1 + ", 2. " + user.answer2 + ", 3. " + user.answer3 + ", 4. " + user.answer4);
            }*/
        }
    }

    IEnumerator GetAnswerData(Action<List<User>> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(url2);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            byte[] result2 = www.downloadHandler.data;
            string jsonResponse = Encoding.UTF8.GetString(result2);
            //Debug.Log("JSON Response: " + jsonResponse); // JSON 응답 확인
            List<User> answers = JsonHelper.FromJson<User>(jsonResponse);
            if (answers != null)
            {
                callback(answers);
            }
            /*foreach (User answer in answers)
            {
                Debug.Log(answer.right);
            }*/
        }
    }
}

// JsonHelper class to handle array responses
public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return new List<T>(wrapper.array);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}
