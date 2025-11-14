using System.Collections;
using TMPro;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// Handles time-based skill check mini-game for cooking
    /// </summary>
    public class SkillCheck : MonoBehaviour
    {
        [Header("UI References")] 
        [SerializeField] [Tooltip("Moving pointer indicator")]
        private RectTransform skillCheckPointer;

        [SerializeField] [Tooltip("Target zone for success")]
        private RectTransform skillCheckZone;
        
        [SerializeField] [Tooltip("Perfect zone for best results")]
        private RectTransform skillCheckPerfectZone;

        [SerializeField] [Tooltip("Background bar")]
        private RectTransform skillCheckBar;

        [SerializeField] [Tooltip("Status text display")]
        private TextMeshProUGUI skillCheckText;

        [Header("Time Settings")] 
        [SerializeField] [Range(1f, 3f)] [Tooltip("Total duration of skill check in seconds")]
        private float totalDuration = 3f;

        [SerializeField] [Range(0f, 1f)] [Tooltip("When success zone starts (0-1, where 0.3 = 30% into duration)")]
        private float successZoneStartTime = 0.4f;

        [SerializeField] [Range(0f, 1f)] [Tooltip("When success zone ends (0-1, where 0.6 = 60% into duration)")]
        private float successZoneEndTime = 0.6f;
        
        [SerializeField] [Range(0f, 1f)] [Tooltip("Perfect zone size relative to success zone (0.5 = 50% of success zone)")]
        private float perfectZoneSizeRatio = 0.4f;

        [Header("Visual Settings")] 
        [SerializeField] private Color successZoneColor = Color.green;
        [SerializeField] private Color perfectZoneColor = Color.yellow;
        [SerializeField] private Color failZoneColor = Color.red;

        private Coroutine _skillCheckRoutine;
        private float _elapsedTime;
        private bool _isComplete;
        private float _perfectZoneStartTime;
        private float _perfectZoneEndTime;

        #region Unity Lifecycle

        private void Awake()
        {
            UpdateZoneVisual();
        }

        private void OnEnable()
        {
            ResetSkillCheck();

            if (_skillCheckRoutine != null) StopCoroutine(_skillCheckRoutine);

            _skillCheckRoutine = StartCoroutine(SkillCheckRoutine());
        }

        private void OnDisable()
        {
            if (_skillCheckRoutine != null)
            {
                StopCoroutine(_skillCheckRoutine);
                _skillCheckRoutine = null;
            }
        }

        #endregion

        #region Skill Check Logic

        /// <summary>
        /// Resets skill check to initial state
        /// </summary>
        private void ResetSkillCheck()
        {
            _elapsedTime = 0f;
            _isComplete = false;
            
            successZoneEndTime = Random.Range(0.5f, 1f);
            successZoneStartTime = successZoneEndTime - Random.Range(0.3f, 0.5f);
            
            // Calculate perfect zone in the center of success zone
            var successZoneCenter = (successZoneStartTime + successZoneEndTime) / 2f;
            var successZoneDuration = successZoneEndTime - successZoneStartTime;
            var perfectZoneDuration = successZoneDuration * perfectZoneSizeRatio;
            
            _perfectZoneStartTime = successZoneCenter - perfectZoneDuration / 2f;
            _perfectZoneEndTime = successZoneCenter + perfectZoneDuration / 2f;
            
            UpdateZoneVisual();

            var barWidth = skillCheckBar.sizeDelta.x;
            skillCheckPointer.localPosition = new Vector3(-barWidth / 2f, 0, 0);

            if (skillCheckText != null) skillCheckText.text = "";
        }

        /// <summary>
        /// Updates visual position of success zone based on time settings
        /// </summary>
        private void UpdateZoneVisual()
        {
            var barWidth = skillCheckBar.sizeDelta.x;

            // Calculate success zone width based on time range
            var zoneWidthRatio = successZoneEndTime - successZoneStartTime;
            var zoneWidth = barWidth * zoneWidthRatio;

            // Calculate success zone position (center of the time range)
            var zoneCenterRatio = (successZoneStartTime + successZoneEndTime) / 2f;
            var zoneCenterX = (zoneCenterRatio - 0.5f) * barWidth;

            skillCheckZone.sizeDelta = new Vector2(zoneWidth, skillCheckZone.sizeDelta.y);
            skillCheckZone.localPosition = new Vector3(zoneCenterX, 0, 0);
            
            // Calculate perfect zone
            if (skillCheckPerfectZone != null)
            {
                var perfectZoneWidthRatio = _perfectZoneEndTime - _perfectZoneStartTime;
                var perfectZoneWidth = barWidth * perfectZoneWidthRatio;
                
                var perfectZoneCenterRatio = (_perfectZoneStartTime + _perfectZoneEndTime) / 2f;
                var perfectZoneCenterX = (perfectZoneCenterRatio - 0.5f) * barWidth;
                
                skillCheckPerfectZone.sizeDelta = new Vector2(perfectZoneWidth, skillCheckPerfectZone.sizeDelta.y);
                skillCheckPerfectZone.localPosition = new Vector3(perfectZoneCenterX, 0, 0);
            }
        }

        /// <summary>
        /// Main skill check coroutine
        /// </summary>
        private IEnumerator SkillCheckRoutine()
        {
            var barWidth = skillCheckBar.sizeDelta.x;
            var startX = -barWidth / 2f;
            var endX = barWidth / 2f;

            while (_elapsedTime < totalDuration)
            {
                _elapsedTime += Time.deltaTime;
                var progress = _elapsedTime / totalDuration;

                // Move pointer
                var pointerX = Mathf.Lerp(startX, endX, progress);
                skillCheckPointer.localPosition = new Vector3(pointerX, 0, 0);

                yield return null;
            }

            // Skill check failed - time ran out
            _isComplete = true;
            ShowResult(false);

            StartCoroutine(CloseAfterDelay());
        }

        /// <summary>
        /// Shows result feedback
        /// </summary>
        private void ShowResult(bool success, bool isPerfect = false)
        {
            if (skillCheckText != null)
            {
                if (isPerfect)
                {
                    skillCheckText.text = "PERFECT!";
                    skillCheckText.color = perfectZoneColor;
                }
                else if (success)
                {
                    skillCheckText.text = "Good!";
                    skillCheckText.color = successZoneColor;
                }
                else
                {
                    skillCheckText.text = "Failed!";
                    skillCheckText.color = failZoneColor;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Call this when player clicks/presses button during skill check
        /// </summary>
        /// <returns>0 = fail, 1 = success, 2 = perfect</returns>
        public SkillCheckResult TryComplete()
        {
            if (_isComplete) return SkillCheckResult.Completed;

            _isComplete = true;

            var progress = _elapsedTime / totalDuration;
            
            // Check for perfect first (it's within success zone)
            var isPerfect = progress >= _perfectZoneStartTime && progress <= _perfectZoneEndTime;
            var isSuccess = progress >= successZoneStartTime && progress <= successZoneEndTime;

            if (isPerfect)
            {
                ShowResult(true, true);
                if (_skillCheckRoutine != null) StopCoroutine(_skillCheckRoutine);
                StartCoroutine(CloseAfterDelay());
                return SkillCheckResult.Perfect; // Perfect
            }
            
            if (isSuccess)
            {
                ShowResult(true, false);
                if (_skillCheckRoutine != null) StopCoroutine(_skillCheckRoutine);
                StartCoroutine(CloseAfterDelay());
                return SkillCheckResult.Success; // Success
            }

            ShowResult(false, false);
            if (_skillCheckRoutine != null) StopCoroutine(_skillCheckRoutine);
            StartCoroutine(CloseAfterDelay());
            return SkillCheckResult.Fail; // Fail
        }
        
        /// <summary>
        /// Closes skill check after showing result
        /// </summary>
        private IEnumerator CloseAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            gameObject.SetActive(false);
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure end time is after start time
            if (successZoneEndTime <= successZoneStartTime)
                successZoneEndTime = Mathf.Min(successZoneStartTime + 0.1f, 1f);

            // Update visual in editor
            if (skillCheckBar && skillCheckZone && skillCheckPerfectZone) UpdateZoneVisual();
        }
#endif

        #endregion
    }
}

public enum SkillCheckResult
{
    Completed,
    Fail,
    Success,
    Perfect
}

