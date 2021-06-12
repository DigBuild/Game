#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(set = 1, binding = 0) uniform sampler2D tex;

layout(location = 0) in vec2 fragUV;
layout(location = 1) in vec4 fragColor;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outBloomColor;
layout(location = 2) out vec4 outWater;
layout(location = 3) out vec4 outNormal;
layout(location = 4) out vec4 outPosition;

void main() {
    outColor = texture(tex, fragUV) * fragColor;
    if (outColor.a < 0.05)
        discard;
    outBloomColor = outWater = outNormal = outPosition = vec4(0);
}
