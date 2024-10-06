using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTargetShower : MonoBehaviour {


    public GameObject tip;
    public LineRenderer lineRenderer;
    private const int posCount = 30;
    private Vector3[] positions = new Vector3[posCount];
    void Start() {
        lineRenderer.enabled = false;
        tip.gameObject.SetActive(false);
        isActive = false;
        lineRenderer.positionCount = posCount;
    }

    private bool isActive = false;
    private bool isActuallyActive = false;
    void _SetActiveStatus(bool _isActive) {
        isActive = _isActive;
        if (isActuallyActive != _isActive) {
            isActuallyActive = _isActive;
            
            lineRenderer.enabled = isActuallyActive;
            tip.gameObject.SetActive(isActuallyActive);
        }
    }


    private Transform lastSourceTransform;
    private Vector3 lastSourcePos;
    private Vector3 lastTargetPos;
    public void ShowTarget(Transform source, Vector3 target) {
        lastSourceTransform = source;
        lastTargetPos = target;
        if (!isActive) {
            tip.transform.position = Vector3.zero;
        }
        
        isActive = true;
        _SetActiveStatus(isActive);
        
    }

    private void Update(){
        if (isActuallyActive) {
            if (lastSourceTransform != null) {
                lastSourcePos = PlayerInteractor.GetCardTipPos(lastSourceTransform);
            } else {
                _SetActiveStatus(false);
                return;
            }
            
            if (!isActive) {
                lastTargetPos = lastSourcePos;
                lastTargetPos.y = 0;
            }
            
            tip.transform.position = Vector3.Lerp(tip.transform.position, lastTargetPos, 25*Time.deltaTime);

            var baseTarget = lastSourcePos;
            var tipTarget = tip.transform.position + Vector3.up * 0.5f;
            for (int i = 0; i < posCount; i++) {
                var percent = i / (posCount - 1f);
                var pos = Vector3.Lerp(baseTarget, tipTarget, percent);
                var sinPercent = percent;
                pos.y += Mathf.Sin(sinPercent * Mathf.PI) * 1;
                positions[i] = pos; 
            }
            
            lineRenderer.material.SetVector("_Offset", new Vector2(-Time.time, 0));
            lineRenderer.SetPositions(positions);
            
            if (!isActive) {
                if (Vector3.Distance(tip.transform.position, lastTargetPos) < 0.2f) {
                    _SetActiveStatus(false);
                }
            }
        }
    }

    public void Stop() {
        isActive = false;
    }

    public void StopImmediately() {
        _SetActiveStatus(false);
    }
}
