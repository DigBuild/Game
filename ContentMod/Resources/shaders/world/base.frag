const vec3 tintDark  = vec3(0.109, 0.121, 0.180);
const vec3 tintLight = vec3(1.000, 1.000, 1.000);

float calculateNormalShade(vec3 normal) {
#ifdef NO_SHADE
    return 1;
#else
    return min(0.7 - 0.2 * abs(normal.x) + 0.5 * normal.y, 1.0);
#endif
}

void compute(vec4 color, vec4 bloom, vec3 normal, float brightness, float sunlight, out vec4 oColor, out vec4 oBloom) {
    float shade = max(calculateNormalShade(normal) * (0.5 + brightness * 0.5), bloom.a);
    vec3 tint = mix(tintDark, tintLight, max(max(sunlight, bloom.a), 0));
    oColor = vec4(color.rgb * shade * tint, color.a);
    oBloom = vec4(bloom.rgb, 1);
}

layout(set = 1, binding = 0) uniform UBO {
	float timeOfDay;
};

layout(set = 2, binding = 1) uniform sampler2D tex;

layout(location = 0) in vec2 fragUV;
layout(location = 1) in vec2 fragBloomUV;
layout(location = 2) in vec3 fragNormal;
layout(location = 3) in vec3 fragPosition;
layout(location = 4) in float fragBrightness;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outBloomColor;
layout(location = 2) out vec4 outWater;
layout(location = 3) out vec4 outNormal;
layout(location = 4) out vec4 outPosition;

void compute(float sunlight) {
    vec4 color = texture(tex, fragUV);
    if (color.a == 0)
        discard;
    vec4 bloom = texture(tex, fragBloomUV);
    compute(color, bloom, fragNormal, fragBrightness, sunlight, outColor, outBloomColor);
    outWater = vec4(0);
    outNormal = vec4(fragNormal, 0);
    outPosition = vec4(fragPosition.xy, 1 - fragPosition.z, 0);
}

void compute() {
    compute(timeOfDay);
}
