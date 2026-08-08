<template>
	<view>
		<up-sticky>
			<view class="searchBox">
				<up-search bgColor="#fff" borderColor="#eee" v-model="query.queryName" placeholder="搜索用户"
					:clearabled="false" :showAction="false" @search="getList(true)"></up-search>
			</view>
		</up-sticky>
		<view class="list" v-if="list.length > 0">
			<view class="item" v-for="item in list" :key="item.Id">
				<view class="left">
					<view>
						<image :src="item.avatar" mode="widthFix"></image>
					</view>
					<view class="info">
						<view class="name">{{ item.nickName }}</view>
						<view class="discs">
							<view class="disc" v-if="item.city">{{ item.city }}</view>
							<view class="disc" v-if="item.gender">
								<image v-if="item.gender == '男'" src="/static/icon23.png" mode="widthFix"></image>
								<image v-if="item.gender == '女'" src="/static/icon24.png" mode="widthFix"></image>
							</view>
						</view>
						<view class="intro" @click="viewIntro(item.intro)">{{ item.intro || '暂无介绍' }}</view>
					</view>
				</view>
				<view class="right">
					<view class="btn" @click="clickSelect(item)">确认选择</view>
				</view>
			</view>
			<view style="height: 24rpx;"></view>
		</view>
		<view class="empty" v-else>
			<image src="/static/img32.png" mode="widthFix"></image>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			query: {
				pageNum: 1,
				pageSize: 10,
				orderNo: "",
				queryName: ""
			},
			total: 0,
			list: []
		};
	},
	onLoad(options) {
		this.query.orderNo = options.orderNo || ""
	},
	onShow() {
		this.getList(true)
	},
	onPullDownRefresh() {
		this.getList(true).then(() => {
			uni.stopPullDownRefresh()
		})
	},
	async onReachBottom() {
		if (this.total > this.query.pageNum * this.query.pageSize) {
			this.query.pageNum += 1
			await this.getList()
		}
	},
	methods: {
		async getList(reset) {
			if (reset) this.query.pageNum = 1
			let res = await this.request('Order/TaskApplyUserList', this.query)
			if (this.query.pageNum == 1) {
				this.list = res.data.items
				this.total = res.data.total
			} else {
				this.list.push(...res.data.items)
			}
		},
		clickSelect(item) {
			this.modal('提示', '确定选择用户吗？', callback => {
				if (callback) {
					this.request('Order/AcceptTaskApply', {
						Id: item.Id
					}).then(res => {
						this.toast("选择用户成功")
						setTimeout(() => {
							uni.redirectTo({
								url: '/pages2ShopPort/order/index?index=4'
							})
						}, 400)
					})
				}
			})
		},
		viewIntro(intro){
			this.modal('个人介绍', intro)
		}
	}
}
</script>

<style lang="scss" scoped>
.searchBox {
	padding: 32rpx;
	padding-top: 32rpx;
	position: relative;
	width: 100%;
	display: flex;
	background: white;
}

.list {
	padding: 0 32rpx;

	.item {
		background: #FFFFFF;
		border-radius: 16rpx;
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-top: 24rpx;
		padding: 20rpx;

		.left {
			display: flex;
			align-items: center;
			flex: 1;
			padding-right: 10rpx;
			image {
				width: 120rpx;
				height: 120rpx;
			}

			.info {
				margin-left: 16rpx;

				.name {
					font-weight: 600;
					font-size: 28rpx;
					color: #333333;
				}

				.discs {
					margin-top: 12rpx;
					display: flex;
					.disc {
						border-radius: 42rpx;
						padding: 12rpx;
						border: 1rpx solid #ECF0F4;
						margin-right: 10rpx;
						font-size: 20rpx;
						color: #666666;
						
						image {
							width: 24rpx;
							height: 24rpx;
						}
					}
				}

				.intro {
					margin-top: 12rpx;
					font-size: 24rpx;
					color: #999999;
					overflow: hidden;
					text-overflow: ellipsis;
					display: -webkit-box;
					-webkit-box-orient: vertical;
					-webkit-line-clamp: 2;
				}
			}
		}

		.right {
			flex-shrink: 1;
			width: 168rpx;
			height: 64rpx;
			border-radius: 16rpx;
			border: 2rpx solid #8C4FFF;
			font-weight: 500;
			font-size: 28rpx;
			color: #8C4FFF;
			display: flex;
			justify-content: center;
			align-items: center;
		}
	}
}
</style>
