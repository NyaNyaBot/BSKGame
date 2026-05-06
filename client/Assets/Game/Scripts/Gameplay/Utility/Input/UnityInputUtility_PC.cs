using System.Collections.Generic;
using Game.Core;
using Game.Gameplay;
using Game.Math;
using GameFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityGameFramework.Runtime;

namespace Game.Client
{
    public class UnityInputUtility_PC: IInputUtility
    {
        private bool m_EnableInPut;
        private PCGameHandle m_PCGameHandle;
        private PCInputCameraHandler m_InputCameraHandler;
        private Dictionary<KeyCode, PlayerPCInput> m_PlayerInputDic = new Dictionary<KeyCode, PlayerPCInput>();
        //上一次处理输入的帧
        private int m_LastExecuteMonoFrame = 0;
        public bool EnableInput
        {
            get=> m_EnableInPut && Initialized;
            set=> m_EnableInPut = value;
        }
        
        public void Init()
        {
            Initialized = true;
            EnableInput = true;
            
            m_PCGameHandle = new PCGameHandle();
            m_PCGameHandle.Init(this);
            m_InputCameraHandler = new PCInputCameraHandler();
        }

        public void Deinit()
        {
            Initialized = false;
        }

        public bool Initialized { get; set; }
        public void Update(float deltaTime)
        {
            if (false == EnableInput)
                return;
            FireMouseRayCast();
            
            ClearOutdatedInputs();
            UpdatePCInput(deltaTime);
            m_InputCameraHandler.Update(deltaTime);
            m_PCGameHandle.ExecuteInput(m_PlayerInputDic, deltaTime);
            m_LastExecuteMonoFrame = TimeUtils.UpdateFrameExecuteCount();
        }

        private void ClearOutdatedInputs()
        {
            using var releaseInputList = new FPoolWrapper<List<PlayerPCInput>, PlayerPCInput>();
            
            foreach (var keyCode in m_PlayerInputDic.Keys)
            {
                var input = m_PlayerInputDic[keyCode];
                if (input.LastUpdateFrameNum <= m_LastExecuteMonoFrame)
                {
                    releaseInputList.Value.Add(input);
                }
            }
            
            foreach (var releasePlayerPCInput in releaseInputList.Value)
            {
                m_PlayerInputDic.Remove(releasePlayerPCInput.KeyCode);
                ReferencePool.Release(releasePlayerPCInput);
            }
        }
        
        private void FireMouseRayCast()
        {
            //检测是否在UI上
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (Camera.main == null)
                return;
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            ImpactInfo impactInfo = null;

            bool hit = PhysicsUtils.SingleLineCheck(mouseRay.origin.ToFloat3(), mouseRay.direction.ToFloat3()
                , 100.0f, InputUtils.MouseRayTraceLayer
                , ref impactInfo, true, duration: 0.0f);

            var eventArgs = ReferencePool.Acquire<MouseRayCastEventArgs>();
            eventArgs.ImpactInfo = impactInfo;
            eventArgs.bIsHit = hit;
            GameEntry.Event.FireNow(this, eventArgs);
            ImpactInfo.Recycle(impactInfo);
        }
        
        
        public float2 GetMoveInputParam(EInputParam param)
        {
            if (EnableInput == false)
            {
                return float2.zero;
            }

            switch (param)
            {
                case EInputParam.kMovementInput:
                    var h = Input.GetAxisRaw("Horizontal");
                    var v = Input.GetAxisRaw("Vertical");
                    return new float2(h, v);
                case EInputParam.kRotationInput:
                    return m_InputCameraHandler.RotationVector.ToFloat2();
                default:
                    Log.Error("Missing case, check");
                    break;
            }

            return float2.zero;
        }

        public bool GetInput(EInputParam moveParam)
        {
            if (EnableInput == false)
            {
                return false;
            }

            switch (moveParam)
            {
                case EInputParam.kJumpBtnInput:
                    return Input.GetKey(KeyCode.Space);
                case EInputParam.kCrouchInput:
                    return Input.GetKey(KeyCode.C);
                case EInputParam.kFireBtnDownInput://execute
                    return GetKeyDown_Execute(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject();
                case EInputParam.kFireBtnUpInput://execute
                    return GetKeyUp_Execute(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject();
                case EInputParam.kIsFiringInput:
                    return Input.GetKey(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject();
                case EInputParam.kAimBtnUpInput:
                    return GetKeyUp_Execute(KeyCode.Mouse1);//execute
                case EInputParam.kAimBtnDownInput:
                    return GetKeyDown_Execute(KeyCode.Mouse1);//execute
                case EInputParam.kIsAimingInput:
                    return Input.GetKey(KeyCode.Mouse1);
                case EInputParam.kReAmmoInput:
                    return GetKeyDown_Execute(KeyCode.R);//execute
                case EInputParam.kSprintBtnDownInput:
                    return GetKeyDown_Execute(KeyCode.LeftShift);//execute
                case EInputParam.kSprintBtnUpInput:
                    return GetKeyUp_Execute(KeyCode.LeftShift);//execute
                case EInputParam.kSprintPressInput:
                    return Input.GetKey(KeyCode.LeftShift);
                case EInputParam.kInteractiveBtnDownInput:
                    return GetKeyDown_Execute(KeyCode.E);//execute
            }

            return false;
        }
        
        private void UpdatePCInput(float deltaTime)
        {
            //检查所有可能的按键 有点粗暴 后续看看怎么改
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                PlayerPCInput playerPCInput = null;
                if (m_PlayerInputDic.ContainsKey(keyCode))
                {
                    playerPCInput= m_PlayerInputDic[keyCode];
                }
                
                // 检查当前按键是否被按下
                //if (Input.GetKeyDown(keyCode))
                if (Input.GetKey(keyCode))
                {
                    if (playerPCInput == null)
                    {
                        playerPCInput = ReferencePool.Acquire<PlayerPCInput>();
                        m_PlayerInputDic.Add(keyCode, playerPCInput);
                        
                        playerPCInput.KeyCode = keyCode;
                        playerPCInput.InitTime = TimeUtils.CurrentTime();
                        playerPCInput.InitFrameNum = TimeUtils.UpdateFrameExecuteCount();
                    }
                    
                    //记录更新帧
                    playerPCInput.LastUpdateFrameNum = TimeUtils.UpdateFrameExecuteCount();
                }
                else
                {
                    if (playerPCInput != null && playerPCInput.MiddleUpFrameNum == 0)
                    {
                        playerPCInput.MiddleUpFrameNum = TimeUtils.UpdateFrameExecuteCount();
                    }
                }
            }
        }
        

        public bool GetKeyDown(EKeyCode keyCode)
        {
            if (false == EnableInput)
                return false;

            return Input.GetKeyDown((KeyCode)(int)keyCode);
        }

        public bool GetKeyUp(EKeyCode keyCode)
        {
            if (false == EnableInput)
                return false;

            return Input.GetKeyUp((KeyCode)(int)keyCode);
        }

        public bool GetKey(EKeyCode keyCode)
        {
            if (false == EnableInput)
                return false;

            return Input.GetKey((KeyCode)(int)keyCode);
        }

        private bool GetKeyDown_Execute(KeyCode keyCode)
        {
            return m_PCGameHandle.GetPlayerPCInputInfo(keyCode).bIsDown;
        }
        
        private bool GetKeyUp_Execute(KeyCode keyCode)
        {
            return m_PCGameHandle.GetPlayerPCInputInfo(keyCode).bIsUp;
        }
    }
}