using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Totem : MonoBehaviour
{
    public const float MaxJumpHeight = 2.5f;

    public const float TransitionDuration = 0.4f;
    public AnimationCurve JumpHeightCurve;
    public AnimationCurve JumpTimeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    List<string> SubObjectsNames = new List<string> { "LowAnimal", "MidAnimal", "HighAnimal" };

    readonly List<GameObject> AnimalObjects = new List<GameObject>(3);
    readonly List<AnimalData> AnimalData = new List<AnimalData>(3);

    NetworkPlayer Owner;

    GameObject Shadow;

    // server-side
    int movementAverageSpeed;
    int moveTimeBuffer;
    readonly List<int> attackTimeBuffers = new List<int>(3);   

    void Start()
    {
        foreach (var n in SubObjectsNames)
            foreach (var r in gameObject.FindChild(n).GetComponentsInChildren<Renderer>())
                r.enabled = false;

        if (Network.isServer)
            TimeKeeper.Instance.Beat += OnBeat;

        transform.position = new Vector3(transform.position.x, 
                                         TerrainGrid.GetHeightAt(transform.position),
                                         transform.position.z);

        Shadow = gameObject.FindChild("Shadow");
    }

    public bool IsEnemy(Totem other)
    {
        return other.Owner != Owner;
    }

    [RPC]
    public void SetOwner(NetworkPlayer owner)
    {
        Owner = owner;
        if (Network.isServer)
            TerrainGrid.RegisterTotem(owner, this);
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

        if (Network.isServer)
            attackTimeBuffers.Add(0);
    }

    void Update()
    {
        var heightAt = TerrainGrid.GetHeightAt(transform.position);
        var distance = transform.position.y - heightAt;

        Shadow.transform.localPosition = new Vector3(0, -distance, 0);

        var scaleFactor = Mathf.Clamp(1 - distance / 3, 0.1f, 1);
        Shadow.transform.localScale = new Vector3(scaleFactor, 1, scaleFactor);
    }

    void OnDestroy()
    {
        TimeKeeper.Instance.Beat -= OnBeat;
        if (Network.isServer)
            TerrainGrid.UnregisterTotem(Owner, this);
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

            // TODO : AI

            // debug -- random direction that doesn't go out of the terrain
            Vector3 direction = Vector3.zero;
            var valid = false;
            foreach (var d in new [] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back }.OrderBy(elem => Guid.NewGuid()))
            {
                var x = (int) Math.Floor(transform.position.x + d.x);
                var z = (int) Math.Floor(transform.position.z + d.z);

                valid |= x >= 0 && x < TerrainGrid.Instance.sizeX &&
                         z >= 0 && z < TerrainGrid.Instance.sizeZ;

                if (valid)
                {
                    direction = d;
                    break;
                }
            }

            if (valid)
                networkView.RPC("MoveTo", RPCMode.All, direction);
        }

        // TODO : If near enemy and wants to attack
        //if (true)
        //    for (int i = 0; i < AnimalObjects.Count; i++)
        //        OnAttackBeat(i, isTriplet);
    }

    void OnAttackBeat(int animalId)
    {
        bool doAttack = false;
        var data = AnimalData[animalId];

        attackTimeBuffers[animalId]++;
        if (attackTimeBuffers[animalId] == 4 && data.speed == 1) doAttack = true;
        else if (attackTimeBuffers[animalId] == 3 && data.speed == 2) doAttack = true;
        else if (attackTimeBuffers[animalId] == 2 && data.speed == 3) doAttack = true;
        else if (attackTimeBuffers[animalId] == 1 && data.speed == 4) doAttack = true;

        if (doAttack)
        {
            attackTimeBuffers[animalId] = 0;

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
            targetHeight = TerrainGrid.GetHeightAt(x, z);
        }
        else
            throw new InvalidOperationException("Trying to move out of the terrain grid");

        var destination = new Vector3(x + 0.5f, targetHeight, z + 0.5f);

        if (Network.isServer)
            TerrainGrid.MoveTotem(Owner, origin, destination);

        TaskManager.Instance.WaitUntil(elapsedTime =>
        {
            var step = Mathf.Clamp01(elapsedTime / TransitionDuration);

            var xzStep = JumpTimeCurve.Evaluate(step);
            var height = JumpHeightCurve.Evaluate(xzStep) + (targetHeight - origin.y) * xzStep;

            transform.position = new Vector3(Mathf.Lerp(origin.x, destination.x, xzStep), height + origin.y,
                                             Mathf.Lerp(origin.z, destination.z, xzStep));

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
