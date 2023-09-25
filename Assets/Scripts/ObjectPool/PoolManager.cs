using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Farm.Audio;

public class PoolManager : MonoBehaviour
{
    public List<GameObject> poolPrefabs;
    private List<ObjectPool<GameObject>> poolEffectList = new List<ObjectPool<GameObject>>();
    // 管理音效的对象池
    private Queue<GameObject> soundQueue = new Queue<GameObject>(); 

    #region Life Function

    private void OnEnable()
    {
        EventHandler.ParticalEffectEvent += OnParticleEffectEvent;
        EventHandler.InitSoundEffectEvent += OnInitSoundEffect;
    }

    private void OnDisable()
    {
        EventHandler.ParticalEffectEvent -= OnParticleEffectEvent;
        EventHandler.InitSoundEffectEvent -= OnInitSoundEffect;
    }

    private void Start()
    {
        CreatePool();
    }

    #endregion

    #region Event Function

    private void OnParticleEffectEvent(ParticalEffectType effectType, Vector3 pos)
    {
        ObjectPool<GameObject> objPool = effectType switch
        {
            ParticalEffectType.LeavesFalling => poolEffectList[0],
            ParticalEffectType.Rock => poolEffectList[1],
            ParticalEffectType.ReapableScenery => poolEffectList[2],
            _ => null,
        };

        GameObject obj = objPool.Get();
        obj.transform.position = pos;
        StartCoroutine(ReleaseRoutine(objPool, obj));
    }

    private void OnInitSoundEffect(SoundDetails soundDetails)
    {
        var obj = GetPoolObject();
        obj.GetComponent<Sound>().SetSound(soundDetails);
        obj.SetActive(true);
        StartCoroutine(DisableSound(obj, soundDetails.soundClip.length));
    }

    #endregion

    /// <summary>
    /// 生成对象池
    /// </summary>
    private void CreatePool()
    {
        foreach (GameObject item in poolPrefabs)
        {
            // 给所有对象池设置一个父物体
            Transform parent = new GameObject(item.name).transform;
            parent.SetParent(transform);

            // 创建对象池，分别设置：创建物体，取出物体，放回物体，销毁物体的执行代码
            var newPool = new ObjectPool<GameObject>(
                () => Instantiate(item, parent),
                e => { e.SetActive(true); },
                e => { e.SetActive(false); },
                e => { Destroy(e); }
            );

            poolEffectList.Add(newPool);
        }
    }

    private IEnumerator ReleaseRoutine(ObjectPool<GameObject> pool, GameObject obj)
    {
        yield return new WaitForSeconds(1.5f);
        pool.Release(obj);
    }

    /// <summary>
    /// 创建音效的对象池
    /// </summary>
    private void CreateSoundPool()
    {
        var parent = new GameObject(poolPrefabs[3].name).transform;
        parent.SetParent(transform);

        // 初始长度为 20
        for (int i = 0; i < 20; i++)
        {
            GameObject newObj = Instantiate(poolPrefabs[3], parent);
            newObj.SetActive(false);
            soundQueue.Enqueue(newObj);
        }
    }

    private GameObject GetPoolObject()
    {
        if (soundQueue.Count < 2)
            CreateSoundPool();
        return soundQueue.Dequeue();
    }

    private IEnumerator DisableSound(GameObject obj, float duration)
    {
        yield return new WaitForSeconds(duration);
        obj.SetActive(false);
        soundQueue.Enqueue(obj);
    }

}