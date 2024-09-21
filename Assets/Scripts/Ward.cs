using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ward : MonoBehaviour
{
    public int num;
    public string WardName;
    public static List<Ward> wards = new List<Ward>();
    public List<DoctorController> doctors = new List<DoctorController>();
    public List<PatientController> inpatients = new List<PatientController>();
    public List<NurseController> nurses = new List<NurseController>();
    public List<PatientController> outpatients = new List<PatientController>();
    public List<BedWaypoint> beds = new List<BedWaypoint>();
    public float totalOfNPC = 0;
    public float infectedNPC = 0;
    public bool isClosed = false;
    public bool isInClosePeriod = false;

    public bool isWaiting = false;

    // Start is called before the first frame update
    void Awake()
    {
        wards.Add(this);
        num = gameObject.name[gameObject.name.Length - 2] - '0';
        Waypoint[] waypoints = transform.GetComponentsInChildren<Waypoint>();
        foreach (Waypoint waypoint in waypoints)
        {
            if (waypoint != null)
            {
                waypoint.ward = num;
            }
        }
        switch (num)
        {
            case 0:
                WardName = "내과 1";
                break;
            case 1:
                WardName = "내과 2";
                break;
            case 2:
                WardName = "외과 1";
                break;
            case 3:
                WardName = "외과 2";
                break;
            case 4:
                WardName = "소아과";
                break;
            case 5:
                WardName = "산부인과";
                break;
        }
        wards.Sort((w1, w2) => w1.num.CompareTo(w2.num));
        Transform waypointsTransform;
        if (num == 8)
        {
            waypointsTransform = transform.Find("EmergencyPatientWaypoints");
        }
        else
        {
            waypointsTransform = transform.Find("InpatientWaypoints");
        }
        if(waypointsTransform != null)
        {
            beds = waypointsTransform.GetComponentsInChildren<BedWaypoint>().ToList();  
        }
        StartCoroutine(WaitOneSecond());
    }

    // Update is called once per frame
    void Update()
    {
        totalOfNPC = doctors.Count + inpatients.Count + nurses.Count + outpatients.Count;
        if (isInClosePeriod || isWaiting)
        {
            return;
        }

        //병동 폐쇄
        //if ((infectedNPC / totalOfNPC >= 0.5) || doctors.Count == 0)
        //{
        //    isClosed = true;
        //}

        if (isClosed)
        {
            StartCoroutine(CloseWard());
        }
    }

    private IEnumerator CloseWard()
    {
        isInClosePeriod = true;
        foreach (PatientController patient in outpatients)
        {
            if (patient != null)
            {
                int index = wards.IndexOf(this);
                if(patient.waypointIndex == 3 || patient.waypointIndex == 4)
                {
                    continue;
                }
                if ((index == 0 || index == 2) && !wards[index + 1].isClosed)
                {
                    patient.ward = index + 1;
                    patient.waypointsTransform = Managers.NPCManager.waypointDictionary[(index + 1, "OutpatientWaypoints")];
                    patient.wardComponent = wards[index + 1];
                    patient.wardComponent.outpatients.Add(patient);
                    patient.waypoints.Clear();
                    patient.isWaitingForDoctor = false;
                    patient.waypointIndex = 0;
                }
                else if ((index == 1 || index == 3) && !wards[index - 1].isClosed)
                {
                    patient.ward = index - 1;
                    patient.waypointsTransform = Managers.NPCManager.waypointDictionary[(index - 1, "OutpatientWaypoints")];
                    patient.wardComponent = wards[index - 1];
                    patient.wardComponent.outpatients.Add(patient);
                    patient.waypoints.Clear();
                    patient.isWaitingForDoctor = false;
                    patient.waypointIndex = 0;
                }
                else
                {
                    patient.StartCoroutine(patient.ExitHospital());
                }

            }
        }
        outpatients.Clear();
        yield return new WaitForSeconds(70);
        isClosed = false;
        isInClosePeriod = false;
    }

    private IEnumerator WaitOneSecond()
    {
        isWaiting = true;
        yield return new WaitForSeconds(1.0f);
        isWaiting = false;
    }
}
