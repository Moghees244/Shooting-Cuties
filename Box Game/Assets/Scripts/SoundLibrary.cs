using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    public SoundGroup[] soundGroups;
    Dictionary<string,AudioClip[]> groupDictionary=new Dictionary<string, AudioClip[]>();

    void Awake()
    {
        foreach(SoundGroup group in soundGroups)
        {
            groupDictionary.Add(group.groupId,group.group);
        }
    }

    public AudioClip getClipByName(string name)
    {
        if(groupDictionary.ContainsKey(name))
        {
            AudioClip[] sounds=groupDictionary[name];
            return sounds[Random.Range(0,sounds.Length)];
        }
        return null;
    }

    [System.Serializable]
    public class SoundGroup
    {
        public string groupId;
        public AudioClip[] group;
    }
}
