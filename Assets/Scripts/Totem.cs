using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Totem : MonoBehaviour
{
    public const float TransitionDuration = 0.1f;

    public delegate void MyTurnHandler(Totem t);
    public event MyTurnHandler MyTurn;

    List<string> SubObjectsNames = new List<string> { "LowAnimal", "MidAnimal", "HighAnimal" };

    readonly List<GameObject> AnimalObjects = new List<GameObject>(3);
    readonly List<AnimalData> AnimalData = new List<AnimalData>(3);

    // server-side
    int movementAverageSpeed;
    int moveTimeBuffer, attackTimeBuffer;

    void Start()
    {
        foreach (var n in SubObjectsNames)
            foreach (var r in gameObject.FindChild(n).GetComponentsInChildren<Renderer>())
                r.enabled = false;

        // Schedule time events
        if (Network.isServer)
            TimeKeeper.Instance.Beat += OnBeat;

        var x = (int) Math.Floor(transform.position.x);
        var z = (int) Math.Floor(transform.position.z);

        transform.position = new Vector3(x + 0.5f, TerrainGrid.Instance.Height[x, z], z + 0.5f);
    }

    [RPC]
    public void AddAnimal(string animalName)
    {
        var animalObject = gameObject.FindChild(SubObjectsNames[0]);
        AnimalObjects.Add(animalObject);

        var animalData = AnimalDatabase.Get(animalName);
        AnimalData.Add(animalData);

        SubObjectsNames.RemoveAt(0);

        var index = animalData.spriteIndex - 1;

        var row = index % 12;
        var col = index / 12;

        foreach (var r in animalObject.GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
            r.material.mainTextureOffset = new Vector2(row / 12f, 1 - col / 12f - 1 / 12f);
        }

        // TODO : floor?
        movementAverageSpeed = (int) Math.Round(AnimalData.Average(x => x.speed));
    }

    void OnDestroy()
    {
        TimeKeeper.Instance.Beat -= OnBeat;
    }

    void OnBeat()
    {
        bool doMove = false;

        moveTimeBuffer++;
        if (moveTimeBuffer == 4 && movementAverageSpeed == 1)       doMove = true;
        else if (moveTimeBuffer == 3 && movementAverageSpeed == 2)  doMove = true;
        else if (moveTimeBuffer == 2 && movementAverageSpeed == 3)  doMove = true;
        else if (moveTimeBuffer == 1 && movementAverageSpeed == 4)  doMove = true;

        if (doMove)
        {
            moveTimeBuffer = 0;
            if(MyTurn != null)
                MyTurn(this);
        }

        // TODO : If near enemy and wants to attack
        //if (true)
        //    for (int i = 0; i < AnimalObjects.Count; i++)
        //        OnAttackBeat(i, isTriplet);
    }

    void OnAttackBeat(int animalId, bool isTriplet)
    {
        bool doAttack = false;
        var data = AnimalData[animalId];

        attackTimeBuffer++;
        if (attackTimeBuffer == 4 && data.speed == 1)       doAttack = true;
        else if (attackTimeBuffer == 3 && data.speed == 2)  doAttack = true;
        else if (attackTimeBuffer == 2 && data.speed == 3)  doAttack = true;
        else if (attackTimeBuffer == 1 && data.speed == 4)  doAttack = true;

        if (doAttack)
        {
            attackTimeBuffer = 0;

            // TODO : grab the enemy's view ID
            NetworkViewID enemyId = default(NetworkViewID);
            networkView.RPC("AttackWith", RPCMode.All, animalId, enemyId);
        }
    }

    [RPC]
    public void MoveTo(Vector3 direction)
    {
        var origin = transform.position;

        var x = (int)Math.Floor(transform.position.x + direction.x);
        var z = (int)Math.Floor(transform.position.z + direction.z);
        float targetHeight;

        if (x >= 0 && x < TerrainGrid.Instance.sizeX &&
            z >= 0 && z < TerrainGrid.Instance.sizeZ)
        {
            targetHeight = TerrainGrid.Instance.Height[x, z];
        }
        else
            throw new InvalidOperationException("Trying to move out of the terrain grid");

        var destination = new Vector3(x + 0.5f, targetHeight, z + 0.5f);

        TaskManager.Instance.WaitUntil(elapsedTime =>
        {
            var step = Mathf.Clamp01(elapsedTime / TransitionDuration);
            var easedStep = Easing.EaseIn(step, EasingType.Quadratic);

            transform.position = Vector3.Lerp(origin, destination, easedStep);

            return step >= 1;
        });
    }

    [RPC]
    public void AttackWith(int animalId, NetworkViewID enemy)
    {
        var animalObject = AnimalObjects[animalId];
        var origin = animalObject.transform.position;

        // TODO : get attack direction from enemy position
        var attackPosition = animalObject.transform.position + Vector3.right * 0.25f; // faked
        animalObject.transform.position = attackPosition;

        // TODO : remove HP from enemy

        // TODO : spawn effect

        TaskManager.Instance.WaitUntil(elapsedTime =>
        {
            var step = Mathf.Clamp01(elapsedTime / TransitionDuration);
            var easedStep = Easing.EaseIn(step, EasingType.Quadratic);

            transform.position = Vector3.Lerp(attackPosition, origin, easedStep);

            return step >= 1;
        });
    }
}
