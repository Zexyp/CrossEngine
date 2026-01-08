float map(float value, float fromMin, float fromMax, float toMin, float toMax) {
	return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
}