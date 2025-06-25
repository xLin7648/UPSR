using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class HitEffectManager : MonoBehaviour
{
    public static HitEffectManager instance;

    public GameObject hitPrefab;

    public Color32 PerfactColor;
    public Color32 GoodColor;

    public IObjectPool<SpriteRenderer> pool;
    private bool collectionChecks;

    private void Awake()
    {
        instance = this;
        pool = new ObjectPool<SpriteRenderer>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 20, 80);
    }

    private void OnDestroyPoolObject(SpriteRenderer obj)
    {
        Destroy(obj.gameObject);
    }

    private void OnReturnedToPool(SpriteRenderer obj)
    {
        obj.gameObject.SetActive(false);
    }

    private void OnTakeFromPool(SpriteRenderer obj)
    {
        obj.gameObject.SetActive(true);
    }

    private SpriteRenderer CreatePooledItem() =>
        Instantiate(hitPrefab).GetComponent<SpriteRenderer>();

    public void Play(bool isPerfact, float noteScale, Transform noteTrans)
    {
        var effectObj = pool.Get();
        var effectTrans = effectObj.transform;

        var ps = effectTrans.GetComponentInChildren<ParticleSystem>().main;
        ps.startColor = new ParticleSystem.MinMaxGradient(
            effectObj.color = isPerfact ? PerfactColor : GoodColor
        );

        effectTrans.position = noteTrans.position;
        effectTrans.position = noteTrans.position + Vector3.back;
        effectTrans.localScale = Vector3.one * (noteScale * 1.35f);
    }
}
