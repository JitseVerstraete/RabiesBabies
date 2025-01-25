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

	[SerializeField] private float _speed = 3f;
	[SerializeField] private float _rotationSpeed = 250f;
	[SerializeField] private float _wallCheckDist = 3f;
	
	private List<Vector3> _linePoints = new List<Vector3>();
	private LineRenderer _lineRenderer;
	private bool _drawing = false;
	private float _lastPointTime;
	private int _pathIndex;
	private float _moveT = 0;
	private float _rotationRemaining;
	private MovementState _state = MovementState.FREE;
	
	
	private void Awake()
	{
		_lineRenderer = GetComponent<LineRenderer>();
		ChangeState(MovementState.FREE);
	}

	private void Update()
	{	
		// for now, always allow going to PATH state except in NONE
		if (Input.GetMouseButtonDown(0) && IsMouseOverThisBaby() && _state != MovementState.NONE)
		{
			ChangeState(MovementState.PATH);
		}
		
		if (_state == MovementState.PATH)
		{
			if (Input.GetMouseButton(0) && _drawing)
			{
				Vector3 worldPos = GetMouseWorldPosition("Floor");
				if (worldPos != Vector3.zero)
					AddPathPoint(worldPos);
				else
					_drawing = false;
			}
			else if (Input.GetMouseButtonUp(0)) 
				_drawing = false;
			else if (_linePoints.Count <= 1)
				ChangeState(MovementState.FREE);

			MoveOnPath();
		}
		else if (_state == MovementState.FREE)
		{
			MoveFreely();
		}
	}

	public void ChangeState(MovementState newState)
	{
		Debug.Log($"New movement state: {newState}");

		// cleanup
		if (_state == MovementState.PATH)
		{
			_lineRenderer.positionCount = 0;
		}
		
		if (newState == MovementState.PATH)
		{
			_linePoints.Clear();
			_drawing = true;
			_pathIndex = 1;
		}
		
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
		
		if (_pathIndex == _linePoints.Count - 1 && !_drawing) ChangeState(MovementState.FREE);
		
		float segmentDistance = Vector3.Distance(_linePoints[_pathIndex-1], _linePoints[_pathIndex]);
		float totalSegmentTime = segmentDistance / _speed; 
		
		_moveT += Time.deltaTime;

		transform.position = Vector3.Lerp(_linePoints[_pathIndex-1], _linePoints[_pathIndex], _moveT / totalSegmentTime);

		UpdateLineSegments();
		
		if (Mathf.Approximately(Vector3.Distance(_linePoints[_pathIndex], transform.position), 0))
		{
			return;
		}
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_linePoints[_pathIndex] - _linePoints[_pathIndex-1]), 10f * Time.deltaTime);
	}

	private void UpdateLineSegments()
	{
		List<Vector3> points = new List<Vector3>(_linePoints);
		points.RemoveRange(0, _pathIndex);
		points.Reverse();
		_lineRenderer.positionCount = points.Count;
		_lineRenderer.SetPositions(points.ToArray());
		
		float totalLength = points.Zip(points.Skip(1), (first, second) => Vector3.Distance(first, second)).Sum();
		
		_lineRenderer.material.mainTextureScale = totalLength * Vector3.one;
		_lineRenderer.material.mainTextureOffset += Vector2.one * Time.deltaTime;
	}

	private void MoveFreely()
	{
		transform.Translate(transform.forward * _speed * Time.deltaTime, Space.World);

		if (!Mathf.Approximately(_rotationRemaining, 0))
		{
			float sign = Mathf.Sign(_rotationRemaining);
			Vector3 rotation = new Vector3(0, sign *70f * Time.deltaTime * 2, 0); // make 70 into seriaz variable
			transform.Rotate(rotation, Space.Self);

			_rotationRemaining -= sign * 70f * Time.deltaTime * 2;
			
			if ((sign > 0 && _rotationRemaining < 0) || (sign < 0 && _rotationRemaining > 0))
				_rotationRemaining = 0;
			return;
		}
		
		
		// move forward
		// 2 raycasts, 45 front left, front right
		// turn rate depending on depth?
		Quaternion rotLeft = transform.rotation * Quaternion.Euler(0, 45, 0);
		Vector3 dirLeft = rotLeft * Vector3.forward;
		float distLeft = GetWallDistance(dirLeft);

		Quaternion rotRight = transform.rotation * Quaternion.Euler(0, -45, 0);
		Vector3 dirRight = rotRight * Vector3.forward;
		float distRight = GetWallDistance(dirRight);

		float dist = (distLeft < distRight) ? distLeft : distRight;
		if (dist < _wallCheckDist)
		{
			// rotate 
			// transform.Rotate(Vector3.up, ((distLeft < distRight) ? -1 : 1 * _rotationSpeed) * distLeft * Time.deltaTime);
			_rotationRemaining = ((distLeft < distRight) ? -1 : 1) * 70f;
		}
		
		// TODO new system - baby keeps rotating after finding wall
		// start a random rotation for a second
		// keep track in _rotationRemaining
	}

	private float GetWallDistance(Vector3 direction)
	{
		Debug.DrawRay(transform.position + Vector3.up*0.2f, direction*1000, Color.red);

		RaycastHit hit;
		if (Physics.Raycast(transform.position, direction, out hit, 100, 1 << LayerMask.NameToLayer("Wall")))
		{
			return hit.distance;
		}
	
		return -1;
	}
}