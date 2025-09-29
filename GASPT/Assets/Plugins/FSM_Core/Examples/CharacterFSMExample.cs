using UnityEngine;
using FSM.Core;
using FSM.Core.Integration;
using GAS.Core;

namespace FSM.Examples
{
    public class CharacterFSMExample : MonoBehaviour
    {
        [Header("컴포넌트 참조")]
        [SerializeField] private StateMachine stateMachine;
        [SerializeField] private GAS.Core.AbilitySystem abilitySystem;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Animator animator;

        [Header("캐릭터 설정")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 10f;

        private bool isGrounded;
        private float horizontalInput;

        private void Awake()
        {
            SetupStateMachine();
        }

        private void Start()
        {
            stateMachine.StartStateMachine("idle");
        }

        private void Update()
        {
            horizontalInput = Input.GetAxis("Horizontal");
            CheckGrounded();
        }

        private void SetupStateMachine()
        {
            IdleState idleState = new IdleState();
            idleState.Initialize("idle", gameObject, stateMachine);

            MoveState moveState = new MoveState();
            moveState.Initialize("move", gameObject, stateMachine);

            JumpState jumpState = new JumpState();
            jumpState.Initialize("jump", gameObject, stateMachine);
            
            AttackState attackState = new AttackState();
            attackState.Initialize("attack", gameObject, stateMachine);

            // 상태 추가
            stateMachine.AddState(idleState);
            stateMachine.AddState(moveState);
            stateMachine.AddState(jumpState);
            stateMachine.AddState(attackState);

            // 전환 추가
            AddTransitions();
        }

        private void AddTransitions()
        {
            // Idle -> Move (움직임 입력 시)
            var idleToMove = new ConditionalTransition("idle_to_move", "idle", "move",
                () => Mathf.Abs(horizontalInput) > 0.1f);
            stateMachine.AddTransition(idleToMove);

            // Move -> Idle (움직임 입력 없을 시)
            var moveToIdle = new ConditionalTransition("move_to_idle", "move", "idle",
                () => Mathf.Abs(horizontalInput) <= 0.1f);
            stateMachine.AddTransition(moveToIdle);

            // Any -> Jump (점프 입력 & 땅에 있을 때)
            foreach (string fromState in new[] { "idle", "move" })
            {
                var toJump = new ConditionalTransition($"{fromState}_to_jump", fromState, "jump",
                    () => Input.GetKeyDown(KeyCode.Space) && isGrounded);
                stateMachine.AddTransition(toJump);
            }

            // Jump -> Idle (착지 시)
            var jumpToIdle = new ConditionalTransition("jump_to_idle", "jump", "idle",
                () => isGrounded && rb.linearVelocity.y <= 0.1f);
            stateMachine.AddTransition(jumpToIdle);

            // Any -> Attack (공격 입력 시)
            foreach (string fromState in new[] { "idle", "move" })
            {
                var toAttack = new ConditionalTransition($"{fromState}_to_attack", fromState, "attack",
                    () => Input.GetKeyDown(KeyCode.Z));
                stateMachine.AddTransition(toAttack);
            }

            // Attack -> Idle (공격 종료 시)
            var attackToIdle = new Transition("attack_to_idle", "attack", "idle");
            attackToIdle.AddCondition(new TimeCondition("attack_duration", 0.5f));
            stateMachine.AddTransition(attackToIdle);
        }

        private void CheckGrounded()
        {
            isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f);
        }

        // 상태 클래스들
        public class IdleState : State
        {
            protected override void OnUpdateState(float deltaTime)
            {
                var example = Owner.GetComponent<CharacterFSMExample>();
                if (example.animator != null)
                {
                    //example.animator.SetBool("IsMoving", false);
                }
            }
        }

        public class MoveState : State
        {
            protected override void OnUpdateState(float deltaTime)
            {
                var example = Owner.GetComponent<CharacterFSMExample>();
                if (example.rb != null)
                {
                    var moveVector = new Vector2(example.horizontalInput * example.moveSpeed, example.rb.linearVelocity.y);
                    example.rb.linearVelocity = moveVector;

                    if (example.animator != null)
                    {
                        example.animator.SetBool("IsMoving", true);
                    }
                }
            }
        }

        public class JumpState : State
        {
            protected override async Awaitable OnEnterState(System.Threading.CancellationToken cancellationToken)
            {
                var example = Owner.GetComponent<CharacterFSMExample>();
                if (example.rb != null)
                {
                    example.rb.linearVelocity = new Vector2(example.rb.linearVelocity.x, example.jumpForce);
                }

                if (example.animator != null)
                {
                    example.animator.SetTrigger("Jump");
                }
                await Awaitable.NextFrameAsync();
            }
        }

        public class AttackState : State
        {
            protected override async Awaitable OnEnterState(System.Threading.CancellationToken cancellationToken)
            {
                var example = Owner.GetComponent<CharacterFSMExample>();

                // 어빌리티 시스템이 있다면 공격 어빌리티 실행
                var abilitySystem = Owner.GetComponent<IAbilitySystem>();
                if (abilitySystem != null)
                {
                    abilitySystem.TryUseAbility("basic_attack");
                }

                if (example.animator != null)
                {
                    example.animator.SetTrigger("Attack");
                }
                await Awaitable.NextFrameAsync();
            }
        }
    }
}
