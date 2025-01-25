using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class BabyMovement : MonoBehaviour
{
	public enum MovementState
	{
		NONE,
		FREE,
		PATH
	}
	
	private List<Vector3> _linePoints = new List<Vector3>();
	private LineRenderer _lineRenderer;
	private bool _drawing = false;
	private float _lastPointTime;
	private int _pathIndex;
	private float _speed = 5f;
	private float _moveT = 0;
	private MovementState _state = MovementState.FREE;
	
	
	private void Awake()
	{
		_lineRenderer = GetComponent<LineRenderer>();
		ChangeState(MovementState.FREE);
	}

	private void Update()
	{	
		// for now, always allow going to PATH state 
		if (Input.GetMouseButtonDown(0) && IsMouseOverThisBaby() && _state != MovementState.NONE)
		{
			_linePoints.Clear();
			ChangeState(MovementState.PATH);
			_drawing = true;
			_pathIndex = 1;
		}
		
		if (_state == MovementState.PATH)
		{
			if (Input.GetMouseButton(0) && _drawing)
				AddPathPoint(GetMouseWorldPosition("Floor"));
			else if (Input.GetMouseButtonUp(0)) 
				_drawing = false;

			MoveOnPath();
		}
	}

	public void ChangeState(MovementState newState)
	{
		_state = newState;
	}

	private Vector3 GetMouseWorldPosition(string layerName)
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100, 1 << LayerMask.NameToLayer(layerName))) {
			return hit.point;
		}
		return Vector3.zero;
	}

	private bool IsMouseOverThisBaby()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100, 1 << LayerMask.NameToLayer("Baby"))) {
			return hit.collider.gameObject == gameObject;
		}
		return false;
	}

	private void AddPathPoint(Vector3 point)
	{
		if (_linePoints.Count > 0 && Vector3.Distance(_linePoints.Last(), point) < .1f) return;
		if (_lastPointTime > 0 && Time.time - _lastPointTime < .02f) return;
		
		point += Vector3.up * .05f;
		_linePoints.Add(point);
		_lineRenderer.positionCount = _linePoints.Count;
		_lineRenderer.SetPositions(_linePoints.ToArray());
		
		_lastPointTime = Time.time;
	}

	private void MoveOnPath()
	{
		if (_linePoints.Count < 2) return;
		
		// move to next point
		float dist = Vector3.Distance(transform.position, _linePoints[_pathIndex]);
		while (dist < 0.01f && _pathIndex < _linePoints.Count-1)
		{
			_pathIndex++;
			_moveT = 0;
			dist = Vector3.Distance(transform.position, _linePoints[_pathIndex]);
		}
		float segmentDistance = Vector3.Distance(_linePoints[_pathIndex-1], _linePoints[_pathIndex]);
		float totalSegmentTime = segmentDistance / _speed; 
		
		
		_moveT += Time.deltaTime;
		// Debug.Log($"Moving to point: {_pathIndex}, distance: {dist}, segment: {segmentDistance}, t: {_moveT}");

		transform.position = Vector3.Lerp(_linePoints[_pathIndex-1], _linePoints[_pathIndex], _moveT / totalSegmentTime);

		if (Mathf.Approximately(Vector3.Distance(_linePoints[_pathIndex], transform.position), 0)) return;
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_linePoints[_pathIndex] - _linePoints[_pathIndex-1]), 10f * Time.deltaTime);
	}
}