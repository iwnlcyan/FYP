using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataMgr :Singleton<DataMgr>
{

    AttributeDesgin[] config_AttributeDesgin;

    public AttributeDesgin LoadAttributeFromDesgin(uint level)
    {
        if (config_AttributeDesgin==null)
        {
            var jsonData = ResourcesExt.Load<TextAsset>("GameDesignerData/PlayerAttribute").text;

            config_AttributeDesgin = JsonConvert.DeserializeObject<AttributeDesgin[]>(jsonData);

        }


        return config_AttributeDesgin[level];

    }
}
