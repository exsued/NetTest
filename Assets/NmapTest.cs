using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using SaltwaterTaffy;
using SaltwaterTaffy.Container;
using Simple.DotNMap;
using System.Net;
using System;
using Random = UnityEngine.Random;

public class NmapTest : MonoBehaviour
{
    string targetIP = "192.168.0.0/24";
    public Transform root = null;
    public GameObject linePrefab = null;
    public GameObject nodePrefab = null;
    private List<GameObject> nodes = new List<GameObject>();
    private List<GameObject> lines = new List<GameObject>();
    IPAddress[] myIPs = null;

    public float spacing = 300f;

    int totalArHosts = 0;

    IEnumerator scan()
    {
        var target = new Target(targetIP);
        var result = new Scanner(target, System.Diagnostics.ProcessWindowStyle.Normal).FastScan();
        var hosts = result.ToArray();

        myIPs = GetMyIPs();
        foreach(var node in nodes)
        {
            Destroy(node);
        }
        nodes.Clear();

        foreach (var line in lines)
        {
            Destroy(line);
        }
        lines.Clear();

        totalArHosts = 0;
        foreach (var host in hosts)
        {
            AddNode(host, ref hosts);
            yield return null;
        }
    }
    public void AddNode(Host host, ref Host[] hosts)
    {
        var isMyHost = myIPs.Contains(host.Address);
        var node = Spawn(host, isMyHost);

        var hostsAroundCount = hosts.Length - 1;

        var angle = 360.0f / hostsAroundCount;
        angle *= Mathf.Deg2Rad;
        if (isMyHost)
            node.localPosition = Vector3.zero;
        else
        {
            var finalPos = new Vector2(Mathf.Cos(angle * totalArHosts), Mathf.Sin(angle * totalArHosts));
            node.GetComponent<RectTransform>().anchoredPosition =
                finalPos.normalized * spacing;
            node.name = totalArHosts.ToString();
            totalArHosts++;

            var line = Instantiate(linePrefab, root);

            lines.Add(line);
            line.transform.SetSiblingIndex(0);
            line.GetComponent<UILineRenderer>().points[1] = node.transform.localPosition;
        }
    }
    public IPAddress[] GetMyIPs()
    {
        var strHostName = string.Empty;
        strHostName = Dns.GetHostName();
        IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
        IPAddress[] addr = ipEntry.AddressList;
        return ipEntry.AddressList;
    }
    public Transform Spawn(Host host, bool isMyHost = false)
    {
        var node = Instantiate(nodePrefab, root);
        var info = node.GetComponentInChildren<Text>();
        info.text = host.Address.ToString();
    
        if(isMyHost)    info.color = Color.green;
        nodes.Add(node);
        return node.transform;
    }
    public void OnScanButtonClick()
    {
        print(string.Format("Initializing scan of {0}", targetIP));
        StartCoroutine(scan());
    }
}
