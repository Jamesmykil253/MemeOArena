using System;
using UnityEngine;

namespace MOBA.Networking
{
    /// <summary>
    /// Input command sent from client to server each tick.
    /// Contains sequence number and input bits for deterministic replay.
    /// </summary>
    [Serializable]
    public struct InputCmd
    {
        public uint sequenceNumber;
        public Vector2 moveInput;
        public bool jumpPressed;
        public bool ability1Pressed;
        public bool ability2Pressed;
        public bool ultimatePressed;
        public bool scoringPressed;
        
        public InputCmd(uint seq, Vector2 move, bool jump, bool ab1, bool ab2, bool ult, bool score)
        {
            sequenceNumber = seq;
            moveInput = move;
            jumpPressed = jump;
            ability1Pressed = ab1;
            ability2Pressed = ab2;
            ultimatePressed = ult;
            scoringPressed = score;
        }
    }

    /// <summary>
    /// Authoritative snapshot sent from server to clients.
    /// Contains last processed input sequence and compressed state data.
    /// </summary>
    [Serializable]
    public struct Snapshot
    {
        public uint lastProcessedSeq;
        public uint tick;
        public Vector3 position;
        public Vector3 velocity;
        public float ultimateEnergy;
        public int carriedPoints;
        public int currentHP;
        public byte locomotionState; // FSM state as byte for compression
        public byte abilityState;
        public byte scoringState;
        
        public Snapshot(uint seq, uint gameTick, Vector3 pos, Vector3 vel, 
                       float energy, int points, int hp, 
                       byte locState, byte abState, byte scoreState)
        {
            lastProcessedSeq = seq;
            tick = gameTick;
            position = pos;
            velocity = vel;
            ultimateEnergy = energy;
            carriedPoints = points;
            currentHP = hp;
            locomotionState = locState;
            abilityState = abState;
            scoringState = scoreState;
        }
    }

    /// <summary>
    /// Discrete event broadcast from server when significant actions occur.
    /// These events are deterministic and can be replayed offline.
    /// </summary>
    [Serializable]
    public struct GameEvent
    {
        public enum EventType : byte
        {
            PlayerKill = 0,
            ScoreDeposit = 1,
            UltimateUsed = 2,
            ObjectiveCaptured = 3,
            PlayerSpawned = 4,
            PlayerDisconnected = 5
        }
        
        public EventType eventType;
        public uint tick;
        public string playerId;
        public string targetId; // for kills, assists
        public int value; // for scores, damage amounts
        public Vector3 position; // event location
        
        public GameEvent(EventType type, uint gameTick, string player, string target = "", int val = 0, Vector3 pos = default)
        {
            eventType = type;
            tick = gameTick;
            playerId = player;
            targetId = target;
            value = val;
            position = pos;
        }
    }
}
