#version 450

layout(binding = 0) uniform UBO {
	float pixelSize;
};

layout(location = 0) in vec2 pos;

layout(location = 0) out vec2 fragSampledUVs[5];

void main() {
    gl_Position = vec4((pos - vec2(0.5, 0.5)) * 2, 0.0, 1.0);

    for (int i = -2; i < 3; i++) {
        fragSampledUVs[i + 2] = pos + vec2(pixelSize * i, 0);
    }
}
