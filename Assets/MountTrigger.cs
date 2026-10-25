using UnityEngine;

public class MountTrigger : MonoBehaviour
{
	public HorseController horse;
	public KeyCode mountKey = KeyCode.E;

	private Transform playerBody;

	public bool isMounted = false;
	public bool canMount = false;

	private void Start(){

	playerBody = GameObject.FindGameObjectWithTag("Player").transform;
	
	}

	private void Update(){
		if(Input.GetKeyDown(mountKey) && canMount){
			if (!isMounted && horse != null){
				isMounted = true;
				horse.Mount(playerBody);
			}
			else if (isMounted){
				isMounted = false;
				horse.Dismount();
			}
		}

	}

	private void OnTriggerEnter(Collider other){
		if(other.CompareTag("Player")){
			canMount = true;
		}
	}
	
	private void OnTriggerExit(Collider other){
		if(other.CompareTag("Player")){
			canMount = false;
		}
	}
}
