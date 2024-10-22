using System.Collections.Generic;
using UnityEngine;

public class AnimationSync : MonoBehaviour
{
    public static Dictionary<EntityEnum, Dictionary<AnimE, AnimationSyncFloat>> Counters;
    public static GameObject Initialize(){
        GameObject obj = new("AnimationSync");
        obj.AddComponent<AnimationSync>();
        Counters = new Dictionary<EntityEnum, Dictionary<AnimE, AnimationSyncFloat>>();
        return obj;
    } 

    public static AnimationSyncFloat GetCounter(EntityEnum entity, AnimE anim){
        if(!Counters.ContainsKey(entity))
            Counters.Add(entity, new Dictionary<AnimE, AnimationSyncFloat>());
        if(!Counters[entity].ContainsKey(anim))
            Counters[entity].Add(anim, new AnimationSyncFloat());
        return Counters[entity][anim];
    }
    void Update()
    {
        foreach(KeyValuePair<EntityEnum, Dictionary<AnimE, AnimationSyncFloat>> entity in Counters){
            foreach(KeyValuePair<AnimE, AnimationSyncFloat> anim in entity.Value){
                AnimationSyncFloat counter = anim.Value;
                counter.Counter += Time.deltaTime;
            }
        }
    }
}