using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class ArcListCycle : MonoBehaviour
{
    public bool m_bTweenScale = false;
    public float m_moveTime = 0.5f;
    public int m_targetItem;
    public Transform[] road;
    public class Item
    {
        public int m_n32Index;
        public GameObject m_pobjGameObject;
    }
    public List<Item> m_objRecords = new List<Item>();
    public GameObject m_objPrefab;
    int m_n32Count;
    public delegate void UpdateCallback(int index, GameObject change);
    UpdateCallback m_UpdateCallback;

    public int RoadLenth { get { return road.Length; } }
    public GameObject TargetObject { get { return m_targetItem >= m_objRecords.Count ? null : m_objRecords[m_targetItem].m_pobjGameObject; } }

    public void Init(int count, UpdateCallback callback)
    {
        Clear();
        m_UpdateCallback = callback;
        if ((m_n32Count = count) < 1) return;
        m_targetItem = Mathf.Clamp(m_targetItem, 0, road.Length - 1);
        for (int i = 0, length = road.Length; i < length; ++i)
        {
            GameObject obj = Instantiate(m_objPrefab) as GameObject;
            obj.transform.parent = road[i];
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.name = i.ToString("000");

            int index = (i - m_targetItem) % count;
            if (index < 0) index += count;

            m_UpdateCallback(index, obj);
            m_objRecords.Add(new Item() { m_n32Index = index, m_pobjGameObject = obj });
        }
    }
    public void Clear()
    {
        for (int i = 0, count = m_objRecords.Count; i < count; ++i)
        {
            Destroy(m_objRecords[i].m_pobjGameObject);
            road[i].transform.DetachChildren();
        }
        m_objRecords.Clear();
    }
    /// <summary>
    /// 用法：selected = ScrollUp();
    /// </summary>
    /// <returns></returns>
    public GameObject ScrollUp()
    {
        return ScrollUp(null);
    }
    /// <summary>
    /// 用法：selected = ScrollUp();
    /// </summary>
    /// <returns></returns>
    public GameObject ScrollUp(TweenCallback callback)
    {
        Tweener tween = null;
        for (int i = 0, length = road.Length - 1; i < length; ++i)
        {
            m_objRecords[i].m_pobjGameObject.transform.parent = road[i + 1];
            (tween = m_objRecords[i].m_pobjGameObject.transform.DOLocalMove(Vector3.one, m_moveTime)).SetEase(Ease.InOutQuad);
            if (m_bTweenScale)
                (m_objRecords[i].m_pobjGameObject.transform.DOScale(Vector3.one, m_moveTime)).SetEase(Ease.InOutQuad);
        }
        if (tween != null) tween.onComplete = callback;
        Item item = m_objRecords[m_objRecords.Count - 1];
        m_objRecords.RemoveAt(m_objRecords.Count - 1);

        int index = m_objRecords[0].m_n32Index - 1;
        if (index < 0) index = m_n32Count - 1;
        item.m_n32Index = index;
        m_objRecords.Insert(0, item);
        item.m_pobjGameObject.transform.parent = road[0];
        item.m_pobjGameObject.transform.localPosition = Vector3.zero;

        m_UpdateCallback(index, item.m_pobjGameObject);
        return TargetObject;
    }
    /// <summary>
    /// 用法：selected = ScrollDown();
    /// </summary>
    /// <returns></returns>
    public GameObject ScrollDown()
    {
        return ScrollDown(null);
    }
    /// <summary>
    /// 用法：selected = ScrollDown();
    /// </summary>
    /// <returns></returns>
    public GameObject ScrollDown(TweenCallback callback)
    {
        Tweener tween = null;
        for (int i = 1, length = road.Length; i < length; ++i)
        {
            m_objRecords[i].m_pobjGameObject.transform.parent = road[i - 1];
            (tween = m_objRecords[i].m_pobjGameObject.transform.DOLocalMove(Vector3.zero, m_moveTime)).SetEase(Ease.InOutQuad);
            if (m_bTweenScale)
                (m_objRecords[i].m_pobjGameObject.transform.DOScale(Vector3.one, m_moveTime)).SetEase(Ease.InOutQuad);
        }
        if (tween != null) tween.onComplete = callback;
        Item item = m_objRecords[0];
        m_objRecords.RemoveAt(0);

        int index = item.m_n32Index = (m_objRecords[m_objRecords.Count - 1].m_n32Index + 1) % m_n32Count;
        m_objRecords.Add(item);
        item.m_pobjGameObject.transform.parent = road[road.Length - 1];
        item.m_pobjGameObject.transform.localPosition = Vector3.zero;

        m_UpdateCallback(index, item.m_pobjGameObject);
        return TargetObject;
    }
    public GameObject Scroll(bool isUp) { return Scroll(isUp, null); }
    public GameObject Scroll(bool isUp, TweenCallback callback)
    {
        return isUp ? ScrollUp(callback) : ScrollDown(callback);
    }
    public void ScrollTo(int index, TweenCallback callback)
    {
        index = Mathf.Clamp(index, 0, m_n32Count - 1);
        int diff = m_objRecords[m_targetItem].m_n32Index - index;
        if (diff != 0)
        {
            bool isUp = diff > 0 && diff < m_n32Count / 2 || diff < -m_n32Count / 2;
            Scroll(isUp, () => ScrollTo(index, callback));
        }
        else if (callback != null)
            callback();
    }
    public GameObject ScrollToImmediate(int index)
    {
        int i = m_objRecords.Count;
        while (--i > -1)
        {
            int num = (i - m_targetItem + index) % m_n32Count;
            if (num < 0) num += m_n32Count;

            m_UpdateCallback((m_objRecords[i].m_n32Index = num), m_objRecords[i].m_pobjGameObject);
            Transform tsf = m_objRecords[i].m_pobjGameObject.transform;
            tsf.parent = road[i];
            tsf.localPosition = Vector3.zero;
            tsf.localScale = Vector3.one;
        }
        return TargetObject;
    }
}
