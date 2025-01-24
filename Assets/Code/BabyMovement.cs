using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class BabyMovement : MonoBehaviour
{
	enum MovementState
	{
		NONE,
		FREE,
		PATH
	}
	
	private List<Vector3> _linePoints = new List<Vector3>();
	private LineRenderer _lineRenderer;
	private MovementState _state = MovementState.FREE;
	
	
	private void Awake()
	{
		_lineRenderer = GetComponent<LineRenderer>();
		ChangeState(MovementState.NONE);
	}

	private void Update()
	{	
		// for now, always allow going to PATH state 
		if (Input.GetMouseButtonDown(0) && GetMouseWorldPosition("Baby") != Vector3.zero)
		{
			_linePoints.Clear();
			ChangeState(MovementState.PATH);
		}
		
		if (_state == MovementState.PATH)
		{
			if (Input.GetMouseButton(0))
			{
				AddPathPoint(GetMouseWorldPosition("Floor"));
			}
			// MoveOnPath TODO
		}
	}

	private void ChangeState(MovementState newState)
	{
		_state = newState;
	}

	private Vector3 GetMouseWorldPosition(string layerName)
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		// TODO take layer into account
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			return hit.point;
		}
		return Vector3.zero;
	}

	private void AddPathPoint(Vector3 point)
	{
		_linePoints.Add(point);
		_lineRenderer.positionCount = _linePoints.Count;
		_lineRenderer.SetPositions(_linePoints.ToArray());
		
		Debug.Log($"Added point: {_linePoints.Last()}, total: {_linePoints.Count}");
	}
}