using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	[CreateAssetMenu(fileName = "PerkDB", menuName = "TBTK_DB/PerkDB", order = 1)]
	public class PerkDB : ScriptableObject {
		
		public Sprite rscIcon;
		public List<Perk> perkList=new List<Perk>();
		
		public static PerkDB LoadDB(){
			return Resources.Load("DB_TBTK/PerkDB", typeof(PerkDB)) as PerkDB;
		}
		
		
		#region runtime code
		public static PerkDB instance;
		public static PerkDB Init(){
			if(instance!=null) return instance;
			instance=LoadDB();
			return instance;
		}
		
		public static PerkDB GetDB(){ return Init(); }
		public static List<Perk> GetList(){ return Init().perkList; }
		public static Perk GetItem(int index){ Init(); return (index>=0 && index<instance.perkList.Count) ? instance.perkList[index] : null; }
		public static int GetItemID(int index){ Init(); return (index>=0 && index<instance.perkList.Count) ? instance.perkList[index].prefabID : -1; }
		public static int GetCount(){ Init(); return instance.perkList.Count; }
		
		public static List<int> GetPrefabIDList(){ Init();
			List<int> prefabIDList=new List<int>();
			for(int i=0; i<instance.perkList.Count; i++) prefabIDList.Add(instance.perkList[i].prefabID);
			return prefabIDList;
		}
		
		public static Perk GetPrefab(int pID){ Init();
			for(int i=0; i<instance.perkList.Count; i++){
				if(instance.perkList[i].prefabID==pID) return instance.perkList[i];
			}
			return null;
		}
		
		public static int GetPrefabIndex(int pID){ Init();
			for(int i=0; i<instance.perkList.Count; i++){
				if(instance.perkList[i].prefabID==pID) return i;
			}
			return -1;
		}
		public static int GetPrefabIndex(Perk ability){
			if(ability==null) return -1;
			return GetPrefabIndex(ability.prefabID);
		}
		
		
		public static Sprite GetRscIcon(){ return Init().rscIcon; }
		public static void SetRscIcon(Sprite icon){ Init().rscIcon=icon; }
		
		
		public static string[] label;
		public static void UpdateLabel(){
			label=new string[GetList().Count];
			for(int i=0; i<GetList().Count; i++) label[i]=i+" - "+GetList()[i].name;
		}
		#endregion
		
		
		#if UNITY_EDITOR
		[ContextMenu ("Reset PrefabID")]
		public void ResetPrefabID(){
			for(int i=0; i<perkList.Count; i++){
				perkList[i].prefabID=i;
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}
		#endif
		
	}
	
	
}
